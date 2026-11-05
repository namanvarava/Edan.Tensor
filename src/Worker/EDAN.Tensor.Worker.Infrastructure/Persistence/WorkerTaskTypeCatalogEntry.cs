namespace EDAN.Tensor.Worker.Infrastructure.Persistence;

/// <summary>Worker가 실행할 수 있는 작업 유형과 운영 활성 상태를 보관합니다.</summary>
public sealed class WorkerTaskTypeCatalogEntry
{
    private WorkerTaskTypeCatalogEntry() { }

    public WorkerTaskTypeCatalogEntry(string taskType, string? description, DateTimeOffset updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskType);
        if (taskType.Trim().Length > 120)
            throw new ArgumentException("작업 유형은 120자를 초과할 수 없습니다.", nameof(taskType));
        if (description?.Length > 500)
            throw new ArgumentException("작업 유형 설명은 500자를 초과할 수 없습니다.", nameof(description));
        TaskType = taskType.Trim();
        Description = description?.Trim();
        IsEnabled = true;
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
        Version = 1;
    }

    public string TaskType { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public long Version { get; private set; }

    /// <summary>등록된 Handler가 시작될 때 유형을 활성 상태로 멱등 갱신합니다.</summary>
    public void Synchronize(string? description, DateTimeOffset nowUtc)
    {
        if (description?.Length > 500)
            throw new ArgumentException("작업 유형 설명은 500자를 초과할 수 없습니다.", nameof(description));
        Description = description?.Trim();
        IsEnabled = true;
        UpdatedAtUtc = nowUtc.ToUniversalTime();
        Version++;
    }

    /// <summary>운영자가 작업 유형의 신규 등록을 차단합니다. 기존 작업 레코드는 보존됩니다.</summary>
    public void Disable(DateTimeOffset nowUtc)
    {
        IsEnabled = false;
        UpdatedAtUtc = nowUtc.ToUniversalTime();
        Version++;
    }
}
