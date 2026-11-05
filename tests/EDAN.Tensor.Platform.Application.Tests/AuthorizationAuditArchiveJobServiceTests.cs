using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Application.UseCases;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Time;
using Xunit;

namespace EDAN.Tensor.Platform.Application.Tests;

/// <summary>감사 아카이브 작업 상태, 실행 이력 및 실패 재시도를 확인합니다.</summary>
public sealed class AuthorizationAuditArchiveJobServiceTests
{
    [Fact]
    public async Task CreatesArchiveAndPersistsVerificationAttempt()
    {
        var repository = new MemoryJobRepository();
        var store = new MemoryArchiveStore();
        var service = CreateService(repository, store);
        var tenantId = new TenantId(Guid.NewGuid());

        var created = await service.CreateAsync(tenantId, From, To, "tenant-admin");
        var verified = await service.VerifyAsync(tenantId, created.JobId, created.Sha256);
        var details = await service.GetAsync(tenantId, created.JobId);

        Assert.Equal("Completed", created.Status);
        Assert.True(verified!.IsValid);
        Assert.Equal("Verified", details!.Job.Status);
        Assert.Equal(new[] { "Create", "Verify" }, details.Attempts.Select(x => x.Operation));
        Assert.All(details.Attempts, x => Assert.True(x.IsSuccessful));
    }

    [Fact]
    public async Task FailedArchiveCanBeRetriedAndKeepsBothAttempts()
    {
        var repository = new MemoryJobRepository();
        var store = new MemoryArchiveStore(failFirstWrite: true);
        var service = CreateService(repository, store);
        var tenantId = new TenantId(Guid.NewGuid());

        await Assert.ThrowsAsync<IOException>(() => service.CreateAsync(tenantId, From, To, "tenant-admin"));
        var failed = Assert.Single(repository.Jobs.Values);
        Assert.Equal(AuthorizationAuditArchiveJobStatus.Failed, failed.Status);

        var retried = await service.RetryAsync(tenantId, failed.Id);
        var details = await service.GetAsync(tenantId, failed.Id);

        Assert.Equal("Completed", retried!.Status);
        Assert.Equal(1, retried.AttemptCount);
        Assert.Equal(2, details!.Attempts.Count);
        Assert.False(details.Attempts[0].IsSuccessful);
        Assert.True(details.Attempts[1].IsSuccessful);
    }

    [Fact]
    public async Task RejectsRetryForCompletedJob()
    {
        var repository = new MemoryJobRepository();
        var service = CreateService(repository, new MemoryArchiveStore());
        var created = await service.CreateAsync(new TenantId(Guid.NewGuid()), From, To, "tenant-admin");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RetryAsync(new TenantId(created.TenantId), created.JobId));
    }

    private static readonly DateTimeOffset From = DateTimeOffset.Parse("2026-11-01T00:00:00Z");
    private static readonly DateTimeOffset To = DateTimeOffset.Parse("2026-11-02T00:00:00Z");

    private static AuthorizationAuditArchiveJobService CreateService(MemoryJobRepository repository,
        IAuthorizationAuditArchiveStore store)
    {
        var auditRepository = new EmptyAuditRepository();
        var archiveService = new AuthorizationAuditArchiveService(auditRepository, store);
        return new AuthorizationAuditArchiveJobService(archiveService, repository,
            new FakeUnitOfWork(), new FakeClock());
    }

    private sealed class MemoryJobRepository : IAuthorizationAuditArchiveJobRepository
    {
        public Dictionary<Guid, AuthorizationAuditArchiveJob> Jobs { get; } = [];
        public List<AuthorizationAuditArchiveAttempt> Attempts { get; } = [];

        public Task<AuthorizationAuditArchiveJob?> GetAsync(TenantId tenantId, Guid jobId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Jobs.TryGetValue(jobId, out var job) && job.TenantId == tenantId ? job : null);

        public Task<IReadOnlyList<AuthorizationAuditArchiveAttempt>> GetAttemptsAsync(TenantId tenantId,
            Guid jobId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AuthorizationAuditArchiveAttempt>>(
                Attempts.Where(x => x.TenantId == tenantId && x.JobId == jobId).ToArray());

        public void Add(AuthorizationAuditArchiveJob job) => Jobs.Add(job.Id, job);
        public void AddAttempt(AuthorizationAuditArchiveAttempt attempt) => Attempts.Add(attempt);
    }

    private sealed class EmptyAuditRepository : IAuthorizationAuditRepository
    {
        public Task<AuthorizationAuditPage> GetPageAsync(TenantId tenantId, string? subjectId, string? action,
            bool? isAllowed, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, int page, int pageSize,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AuthorizationAuditPage([], 0, page, pageSize));

        public Task<IReadOnlyList<AuthorizationAuditEntry>> GetForIntegrityAsync(TenantId tenantId,
            DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AuthorizationAuditEntry>>([]);

        public void Add(AuthorizationAuditEntry entry) => throw new NotSupportedException();
    }

    private sealed class MemoryArchiveStore(bool failFirstWrite = false) : IAuthorizationAuditArchiveStore
    {
        private readonly Dictionary<Guid, byte[]> _items = [];
        private bool _failed;

        public Task WriteAsync(Guid archiveId, ReadOnlyMemory<byte> content,
            CancellationToken cancellationToken = default)
        {
            if (failFirstWrite && !_failed)
            {
                _failed = true;
                throw new IOException("테스트용 저장소 오류");
            }
            _items.Add(archiveId, content.ToArray());
            return Task.CompletedTask;
        }

        public Task<byte[]?> ReadAsync(Guid archiveId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.TryGetValue(archiveId, out var bytes) ? bytes : null);
    }

    private sealed class FakeUnitOfWork : IPlatformUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class FakeClock : ISystemClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.Parse("2026-11-05T00:00:00Z");
    }
}
