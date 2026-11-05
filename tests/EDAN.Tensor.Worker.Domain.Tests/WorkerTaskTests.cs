using EDAN.Tensor.Worker.Domain.Model;
using Xunit;

namespace EDAN.Tensor.Worker.Domain.Tests;

public sealed class WorkerTaskTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 0, 0, 0, TimeSpan.Zero);
    private static WorkerTask Create(int maxAttempts = 3) =>
        WorkerTask.Create(Guid.NewGuid(), "archive.generate", Guid.NewGuid().ToString("N"),
            "payload://archive/1", Now, maxAttempts);

    [Fact]
    public void NewTaskStartsPending()
    {
        var task = Create();
        Assert.Equal(WorkerTaskStatus.Pending, task.Status);
        Assert.Equal(0, task.FencingToken);
        Assert.Equal(0, task.AttemptCount);
    }

    [Fact]
    public void LeaseAndStartRequireMatchingOwnership()
    {
        var task = Create();
        var leaseId = Guid.NewGuid();
        var token = task.AcquireLease("worker-a", leaseId, Now, Now.AddMinutes(10));
        Assert.Equal(1, token);
        task.Start(leaseId, token, Now.AddMinutes(1));
        Assert.Equal(WorkerTaskStatus.Running, task.Status);
        Assert.Throws<InvalidOperationException>(() => task.SaveCheckpoint(leaseId, token - 1, "offset:1", Now.AddMinutes(2)));
    }

    [Fact]
    public void ExpiredLeaseCannotWriteCheckpoint()
    {
        var task = Create();
        var leaseId = Guid.NewGuid();
        var token = task.AcquireLease("worker-a", leaseId, Now, Now.AddMinutes(1));
        Assert.Throws<InvalidOperationException>(() => task.SaveCheckpoint(leaseId, token, "offset:1", Now.AddMinutes(1)));
    }

    [Fact]
    public void CheckpointCanBeHandedOffAndReacquiredWithHigherToken()
    {
        var task = Create();
        var firstLease = Guid.NewGuid();
        var firstToken = task.AcquireLease("worker-a", firstLease, Now, Now.AddMinutes(10));
        task.Start(firstLease, firstToken, Now);
        task.Handoff(firstLease, firstToken, "offset:42", Now.AddMinutes(1));

        Assert.Equal(WorkerTaskStatus.Pending, task.Status);
        Assert.Equal("offset:42", task.Checkpoint);
        Assert.Null(task.LeaseId);

        var secondToken = task.AcquireLease("worker-b", Guid.NewGuid(), Now.AddMinutes(2), Now.AddMinutes(12));
        Assert.True(secondToken > firstToken);
    }

    [Fact]
    public void LeaseCanBeRenewedOnlyByCurrentOwner()
    {
        var task = Create();
        var leaseId = Guid.NewGuid();
        var token = task.AcquireLease("worker-a", leaseId, Now, Now.AddMinutes(10));
        task.RenewLease(leaseId, token, Now.AddMinutes(5), Now.AddMinutes(20));
        Assert.Equal(Now.AddMinutes(20), task.LeaseExpiresAtUtc);
        Assert.Throws<InvalidOperationException>(() => task.RenewLease(leaseId, token + 1, Now.AddMinutes(6), Now.AddMinutes(30)));
    }

    [Fact]
    public void ExpiredLeaseCannotBeRenewed()
    {
        var task = Create();
        var leaseId = Guid.NewGuid();
        var token = task.AcquireLease("worker-a", leaseId, Now, Now.AddMinutes(1));

        Assert.Throws<InvalidOperationException>(() =>
            task.RenewLease(leaseId, token, Now.AddMinutes(1), Now.AddMinutes(10)));

        Assert.Equal(Now.AddMinutes(1), task.LeaseExpiresAtUtc);
        Assert.Equal(WorkerTaskStatus.Leased, task.Status);
    }

    [Fact]
    public void RenewalWithNonFutureExpiryDoesNotMutateLease()
    {
        var task = Create();
        var leaseId = Guid.NewGuid();
        var token = task.AcquireLease("worker-a", leaseId, Now, Now.AddMinutes(10));
        var now = Now.AddMinutes(2);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            task.RenewLease(leaseId, token, now, now));

        Assert.Equal(Now.AddMinutes(10), task.LeaseExpiresAtUtc);
        Assert.Equal(WorkerTaskStatus.Leased, task.Status);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(10)]
    public void RenewalCannotShortenOrPreserveLeaseExpiry(int expiryMinute)
    {
        var task = Create();
        var leaseId = Guid.NewGuid();
        var token = task.AcquireLease("worker-a", leaseId, Now, Now.AddMinutes(10));
        var version = task.Version;
        var now = Now.AddMinutes(2);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            task.RenewLease(leaseId, token, now, Now.AddMinutes(expiryMinute)));

        Assert.Equal(Now.AddMinutes(10), task.LeaseExpiresAtUtc);
        Assert.Equal(WorkerTaskStatus.Leased, task.Status);
        Assert.Equal(version, task.Version);
    }

    [Fact]
    public void FailedTaskSchedulesRetryUntilAttemptLimitThenMovesToDeadLetter()
    {
        var task = Create(maxAttempts: 2);
        var firstLease = Guid.NewGuid();
        var firstToken = task.AcquireLease("worker-a", firstLease, Now, Now.AddMinutes(10));
        task.Start(firstLease, firstToken, Now);
        task.Fail(firstLease, firstToken, "transient", Now.AddMinutes(1), Now.AddMinutes(2));
        Assert.Equal(WorkerTaskStatus.RetryScheduled, task.Status);

        var secondLease = Guid.NewGuid();
        var secondToken = task.AcquireLease("worker-b", secondLease, Now.AddMinutes(2), Now.AddMinutes(12));
        task.Fail(secondLease, secondToken, "permanent", Now.AddMinutes(3), Now.AddMinutes(4));
        Assert.Equal(WorkerTaskStatus.DeadLettered, task.Status);
        Assert.Null(task.LeaseId);
    }

    [Fact]
    public void CompleteReleasesLeaseAndRecordsCompletion()
    {
        var task = Create();
        var leaseId = Guid.NewGuid();
        var token = task.AcquireLease("worker-a", leaseId, Now, Now.AddMinutes(10));
        task.Start(leaseId, token, Now);
        task.Complete(leaseId, token, Now.AddMinutes(1));
        Assert.Equal(WorkerTaskStatus.Succeeded, task.Status);
        Assert.Null(task.LeaseId);
        Assert.Equal(Now.AddMinutes(1), task.CompletedAtUtc);
    }

    [Fact]
    public void InvalidRetryTimeDoesNotMutateTask()
    {
        var task = Create();
        var leaseId = Guid.NewGuid();
        var token = task.AcquireLease("worker-a", leaseId, Now, Now.AddMinutes(10));
        task.Start(leaseId, token, Now);
        Assert.Throws<ArgumentOutOfRangeException>(() => task.Fail(leaseId, token, "transient", Now.AddMinutes(1), Now));
        Assert.Equal(WorkerTaskStatus.Running, task.Status);
        Assert.Equal(leaseId, task.LeaseId);
    }
}
