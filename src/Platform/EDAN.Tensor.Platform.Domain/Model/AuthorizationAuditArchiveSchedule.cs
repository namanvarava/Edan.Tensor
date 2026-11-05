namespace EDAN.Tensor.Platform.Domain.Model;

/// <summary>테넌트별 감사 아카이브 예약 주기와 분산 실행 임대를 관리합니다.</summary>
public sealed class AuthorizationAuditArchiveSchedule
{
    private AuthorizationAuditArchiveSchedule() { }

    private AuthorizationAuditArchiveSchedule(Guid tenantId, int intervalHours, DateTimeOffset nowUtc)
    {
        if (intervalHours is < 1 or > 744)
            throw new ArgumentOutOfRangeException(nameof(intervalHours), "예약 주기는 1시간부터 31일까지 설정할 수 있습니다.");
        Id = Guid.NewGuid();
        TenantId = new TenantId(tenantId);
        IntervalHours = intervalHours;
        IsEnabled = true;
        NextRunAtUtc = nowUtc.ToUniversalTime().AddHours(intervalHours);
        Version = 1;
    }

    public Guid Id { get; private set; }
    public TenantId TenantId { get; private set; } = null!;
    public int IntervalHours { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTimeOffset NextRunAtUtc { get; private set; }
    public DateTimeOffset? LastRunAtUtc { get; private set; }
    public Guid? LeaseId { get; private set; }
    public DateTimeOffset? LeaseExpiresAtUtc { get; private set; }
    public long Version { get; private set; }

    /// <summary>새 예약 정책을 생성합니다.</summary>
    public static AuthorizationAuditArchiveSchedule Create(TenantId tenantId, int intervalHours,
        DateTimeOffset nowUtc) => new(tenantId.Value, intervalHours, nowUtc);

    /// <summary>예약 주기를 변경하고 다음 실행 시각을 다시 계산합니다.</summary>
    public void Update(int intervalHours, bool isEnabled, DateTimeOffset nowUtc)
    {
        if (intervalHours is < 1 or > 744)
            throw new ArgumentOutOfRangeException(nameof(intervalHours), "예약 주기는 1시간부터 31일까지 설정할 수 있습니다.");
        IntervalHours = intervalHours;
        IsEnabled = isEnabled;
        NextRunAtUtc = nowUtc.ToUniversalTime().AddHours(intervalHours);
        Version++;
    }
}

/// <summary>예약 아카이브 정책 조회 결과를 제공합니다.</summary>
