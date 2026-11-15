using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Domain.Model;

namespace EDAN.Tensor.Worker.Application.Execution;

/// <summary>Tenant 범위 DLQ 조회와 운영자 재처리 요청을 조정합니다.</summary>
public sealed class WorkerTaskDeadLetterService(
    IWorkerTaskDeadLetterRepository repository,
    TimeProvider timeProvider)
{
    public Task<IReadOnlyList<WorkerTask>> GetAsync(Guid tenantId, int page = 1, int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "페이지는 1 이상이어야 합니다.");
        if (pageSize is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "페이지 크기는 1~100이어야 합니다.");
        var skip = checked((page - 1) * pageSize);
        return repository.GetDeadLetteredAsync(tenantId, skip, pageSize, cancellationToken);
    }

    /// <summary>DLQ를 작업 유형·오류·생성 기간으로 필터링해 페이지 조회합니다.</summary>
    public Task<WorkerDeadLetterPage> GetPageAsync(Guid tenantId, string? taskType, string? errorCode,
        DateTimeOffset? fromUtc, DateTimeOffset? toUtc, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));
        _ = checked((page - 1) * pageSize);
        return repository.GetDeadLetteredPageAsync(tenantId, taskType, errorCode, fromUtc, toUtc,
            page, pageSize, cancellationToken);
    }

    /// <summary>Tenant 범위 DLQ 재처리 요청 이력을 기간과 원본 작업으로 조회합니다.</summary>
    public Task<WorkerReplayHistoryPage> GetReplayHistoryAsync(Guid tenantId, Guid? originalTaskId,
        DateTimeOffset? fromUtc, DateTimeOffset? toUtc, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));
        _ = checked((page - 1) * pageSize);
        return repository.GetReplayHistoryAsync(tenantId, originalTaskId, fromUtc, toUtc,
            page, pageSize, cancellationToken);
    }

    /// <summary>같은 요청 ID의 재시도는 기존 재처리 작업을 반환하고, 새 요청 ID는 별도 재처리를 생성합니다.</summary>
    public Task<Guid?> ReplayAsync(Guid tenantId, Guid taskId, Guid replayRequestId, string requestedBy,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        if (taskId == Guid.Empty)
            throw new ArgumentException("작업 식별자는 비어 있을 수 없습니다.", nameof(taskId));
        if (replayRequestId == Guid.Empty)
            throw new ArgumentException("재처리 요청 식별자는 비어 있을 수 없습니다.", nameof(replayRequestId));
        ArgumentException.ThrowIfNullOrWhiteSpace(requestedBy);
        if (requestedBy.Trim().Length > 200)
            throw new ArgumentException("요청자 식별자는 200자를 초과할 수 없습니다.", nameof(requestedBy));
        return repository.ReplayAsync(tenantId, taskId, replayRequestId, requestedBy.Trim(),
            timeProvider.GetUtcNow(), cancellationToken);
    }
}
