using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Time;

namespace EDAN.Tensor.Platform.Application.UseCases;

/// <summary>아카이브 작업 상태, 실행 이력, 재시도 및 검증 결과를 영속화합니다.</summary>
public sealed class AuthorizationAuditArchiveJobService(
    AuthorizationAuditArchiveService archives,
    IAuthorizationAuditArchiveJobRepository jobs,
    IPlatformUnitOfWork unitOfWork,
    ISystemClock clock)
{
    /// <summary>새 아카이브 작업을 등록하고 실행 결과와 시도 이력을 저장합니다.</summary>
    public async Task<AuthorizationAuditArchiveJobView> CreateAsync(TenantId tenantId,
        DateTimeOffset fromUtc, DateTimeOffset toUtc, string actor,
        CancellationToken cancellationToken = default)
    {
        var job = AuthorizationAuditArchiveJob.Create(tenantId, fromUtc, toUtc, actor, clock.UtcNow);
        jobs.Add(job);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await ExecuteAsync(job, "Create", cancellationToken);
    }

    /// <summary>실패한 아카이브 작업을 다시 실행합니다.</summary>
    public async Task<AuthorizationAuditArchiveJobView?> RetryAsync(TenantId tenantId, Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await jobs.GetAsync(tenantId, jobId, cancellationToken);
        if (job is null) return null;
        job.PrepareRetry();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await ExecuteAsync(job, "Retry", cancellationToken);
    }

    /// <summary>아카이브 체크섬을 확인하고 검증 결과를 실행 이력에 기록합니다.</summary>
    public async Task<AuthorizationAuditArchiveJobVerification?> VerifyAsync(TenantId tenantId,
        Guid jobId, string? expectedSha256, CancellationToken cancellationToken = default)
    {
        var job = await jobs.GetAsync(tenantId, jobId, cancellationToken);
        if (job is null) return null;
        if (job.Status != AuthorizationAuditArchiveJobStatus.Completed
            && job.Status != AuthorizationAuditArchiveJobStatus.Verified)
            throw new InvalidOperationException("완료된 아카이브 작업만 검증할 수 있습니다.");

        if (expectedSha256 is not null && (expectedSha256.Length != 64
            || expectedSha256.Any(x => !Uri.IsHexDigit(x))))
            throw new ArgumentException("기준 체크섬은 64자리 16진수여야 합니다.");

        var previousAttempts = await jobs.GetAttemptsAsync(tenantId, jobId, cancellationToken);
        var attempt = AuthorizationAuditArchiveAttempt.Start(job.Id, tenantId,
            previousAttempts.Count + 1, "Verify", clock.UtcNow);
        jobs.AddAttempt(attempt);
        var result = await archives.VerifyAsync(tenantId, job.ArchiveId!.Value,
            expectedSha256 ?? job.Sha256, cancellationToken);
        attempt.Finish(result.IsValid, result.ReasonCode, clock.UtcNow);
        if (result.IsValid) job.MarkVerified();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthorizationAuditJobVerification(job.Id, result.IsValid, result.ReasonCode, result.Sha256);
    }

    /// <summary>작업과 실행 이력을 함께 조회합니다.</summary>
    public async Task<AuthorizationAuditArchiveJobDetails?> GetAsync(TenantId tenantId, Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await jobs.GetAsync(tenantId, jobId, cancellationToken);
        if (job is null) return null;
        var attempts = await jobs.GetAttemptsAsync(tenantId, jobId, cancellationToken);
        return new AuthorizationAuditArchiveJobDetails(ToView(job), attempts.Select(ToAttempt).ToArray());
    }

    private async Task<AuthorizationAuditArchiveJobView> ExecuteAsync(AuthorizationAuditArchiveJob job,
        string operation, CancellationToken cancellationToken)
    {
        var previousAttempts = await jobs.GetAttemptsAsync(job.TenantId, job.Id, cancellationToken);
        var attempt = AuthorizationAuditArchiveAttempt.Start(job.Id, job.TenantId,
            previousAttempts.Count + 1, operation, clock.UtcNow);
        jobs.AddAttempt(attempt);
        AuthorizationAuditArchiveResult result;
        try
        {
            result = await archives.CreateAsync(job.TenantId, job.FromUtc, job.ToUtc, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            job.Fail(ex.GetType().Name);
            attempt.Finish(false, ex.GetType().Name, clock.UtcNow);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }

        job.Complete(result.ArchiveId, result.RecordCount, result.Sha256, result.SizeBytes, clock.UtcNow);
        attempt.Finish(true, "archive.created", clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToView(job);
    }

    private static AuthorizationAuditArchiveJobView ToView(AuthorizationAuditArchiveJob x) =>
        new(x.Id, x.TenantId.Value, x.FromUtc, x.ToUtc, x.RequestedBy, x.CreatedAtUtc,
            x.Status.ToString(), x.ArchiveId, x.RecordCount, x.Sha256, x.SizeBytes,
            x.CompletedAtUtc, x.FailureReason, x.AttemptCount, x.Version);

    private static AuthorizationAuditArchiveAttemptView ToAttempt(AuthorizationAuditArchiveAttempt x) =>
        new(x.Id, x.AttemptNumber, x.Operation, x.StartedAtUtc, x.FinishedAtUtc, x.IsSuccessful, x.ResultCode);
}

/// <summary>아카이브 작업의 현재 상태를 제공합니다.</summary>
public sealed record AuthorizationAuditArchiveJobView(Guid JobId, Guid TenantId, DateTimeOffset FromUtc,
    DateTimeOffset ToUtc, string RequestedBy, DateTimeOffset CreatedAtUtc, string Status,
    Guid? ArchiveId, int RecordCount, string? Sha256, int? SizeBytes,
    DateTimeOffset? CompletedAtUtc, string? FailureReason, int AttemptCount, long Version);

/// <summary>아카이브 작업과 실행 시도 목록을 함께 제공합니다.</summary>
public sealed record AuthorizationAuditArchiveJobDetails(AuthorizationAuditArchiveJobView Job,
    IReadOnlyList<AuthorizationAuditArchiveAttemptView> Attempts);

/// <summary>아카이브 작업의 체크섬 검증 결과를 제공합니다.</summary>
public sealed record AuthorizationAuditJobVerification(Guid JobId, bool IsValid, string ReasonCode, string? Sha256);

/// <summary>아카이브 작업 실행 시도 결과를 제공합니다.</summary>
public sealed record AuthorizationAuditArchiveAttemptView(Guid Id, int AttemptNumber, string Operation,
    DateTimeOffset StartedAtUtc, DateTimeOffset? FinishedAtUtc, bool? IsSuccessful, string? ResultCode);
