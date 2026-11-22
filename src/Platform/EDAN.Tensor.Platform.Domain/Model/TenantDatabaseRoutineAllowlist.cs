using EDAN.Tensor.SharedKernel.Primitives;

namespace EDAN.Tensor.Platform.Domain.Model;

public sealed record TenantDatabaseRoutineAllowlistId : StronglyTypedId<Guid>
{
    public TenantDatabaseRoutineAllowlistId(Guid value) : base(value == Guid.Empty ? throw new ArgumentException("루틴 허용 목록 식별자가 필요합니다.") : value) { }
}

public enum TenantDatabaseRoutineKind { Procedure, Function, OraclePackageProcedure }

public sealed class TenantDatabaseRoutineAllowlist : AggregateRoot<TenantDatabaseRoutineAllowlistId>
{
    private TenantDatabaseRoutineAllowlist() : base(new TenantDatabaseRoutineAllowlistId(Guid.NewGuid())) { }
    private TenantDatabaseRoutineAllowlist(TenantDatabaseRoutineAllowlistId id, TenantId tenantId, TenantDatabaseRoutineKind kind, string schemaName, string routineName, DateTimeOffset now) : base(id)
    {
        TenantId = tenantId; Kind = kind; SchemaName = Required(schemaName, nameof(schemaName)); RoutineName = Required(routineName, nameof(routineName)); IsEnabled = true; CreatedAtUtc = now.ToUniversalTime();
    }
    public TenantId TenantId { get; private set; } = null!;
    public TenantDatabaseRoutineKind Kind { get; private set; }
    public string SchemaName { get; private set; } = string.Empty;
    public string RoutineName { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public static TenantDatabaseRoutineAllowlist Create(TenantDatabaseRoutineAllowlistId id, TenantId tenantId, TenantDatabaseRoutineKind kind, string schemaName, string routineName, DateTimeOffset now) => new(id, tenantId, kind, schemaName, routineName, now);
    public void Disable(DateTimeOffset now) { IsEnabled = false; UpdatedAtUtc = now.ToUniversalTime(); }
    public void Enable(DateTimeOffset now) { IsEnabled = true; UpdatedAtUtc = now.ToUniversalTime(); }
    private static string Required(string value, string name) { if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 255) throw new ArgumentException("유효하지 않은 루틴 이름입니다.", name); return value.Trim(); }
}
