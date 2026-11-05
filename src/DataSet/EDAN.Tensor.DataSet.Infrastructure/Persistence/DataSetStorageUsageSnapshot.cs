namespace EDAN.Tensor.DataSet.Infrastructure.Persistence;

/// <summary>Tenant별 실제 콘텐츠 저장량을 시간 단위로 기록하는 재시도 안전 스냅샷입니다.</summary>
public sealed class DataSetStorageUsageSnapshot
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTimeOffset PeriodStartUtc { get; set; }
    public DateTimeOffset CapturedAtUtc { get; set; }
    public long UsedBytes { get; set; }
    public long DataSetCount { get; set; }
}
