namespace EDAN.Tensor.DataSet.Infrastructure.Persistence;

/// <summary>Quota 상태가 경계를 통과할 때만 추가되는 Tenant별 이벤트 이력입니다.</summary>
public sealed class DataSetQuotaThresholdEvent
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PolicyId { get; set; }
    public string Scope { get; set; } = "Tenant";
    public Guid TargetId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public long UsedBytes { get; set; }
    public long LimitBytes { get; set; }
    public double UtilizationPercent { get; set; }
    public long PolicyVersion { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
}
