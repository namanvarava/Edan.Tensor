using EDAN.Tensor.Worker.Domain.Model;

namespace EDAN.Tensor.Worker.Application.Abstractions;

/// <summary>DLQ 조회와 요청 단위 멱등성 이력 보존형 재처리를 제공합니다.</summary>
public interface IWorkerTaskDeadLetterRepository
{
    Task<IReadOnlyList<WorkerTask>> GetDeadLetteredAsync(Guid tenantId, int skip, int take,
        CancellationToken cancellationToken = default);
    Task<WorkerDeadLetterPage> GetDeadLetteredPageAsync(Guid tenantId, string? taskType,
        string? errorCode, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, int page, int pageSize,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException("DLQ 페이지 조회가 구현되지 않았습니다.");
    Task<WorkerReplayHistoryPage> GetReplayHistoryAsync(Guid tenantId, Guid? originalTaskId,
        DateTimeOffset? fromUtc, DateTimeOffset? toUtc, int page, int pageSize,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException("DLQ 재처리 이력 조회가 구현되지 않았습니다.");
    Task<Guid?> ReplayAsync(Guid tenantId, Guid taskId, Guid replayRequestId, string requestedBy,
        DateTimeOffset nowUtc, CancellationToken cancellationToken = default);
}

/// <summary>DLQ 항목의 필터·페이징 결과입니다.</summary>
public sealed record WorkerDeadLetterPage(
    IReadOnlyList<WorkerTask> Items, int TotalCount, int Page, int PageSize);

/// <summary>DLQ 재처리 이력의 API projection입니다.</summary>
public sealed record WorkerReplayHistoryView(
    Guid Id, Guid OriginalTaskId, Guid ReplayTaskId, Guid ReplayRequestId,
    string RequestedBy, DateTimeOffset CreatedAtUtc);

/// <summary>DLQ 재처리 이력의 필터·페이징 결과입니다.</summary>
public sealed record WorkerReplayHistoryPage(
    IReadOnlyList<WorkerReplayHistoryView> Items, int TotalCount, int Page, int PageSize);
