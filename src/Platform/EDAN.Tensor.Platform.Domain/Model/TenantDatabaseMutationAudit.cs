using EDAN.Tensor.SharedKernel.Primitives;

namespace EDAN.Tensor.Platform.Domain.Model;

public sealed record TenantDatabaseMutationAuditId : StronglyTypedId<Guid>
{
    public TenantDatabaseMutationAuditId(Guid value) : base(value == Guid.Empty ? throw new ArgumentException("변경 감사 식별자가 필요합니다.") : value) { }
}

public sealed class TenantDatabaseMutationAudit : AggregateRoot<TenantDatabaseMutationAuditId>
{
    private TenantDatabaseMutationAudit() : base(new TenantDatabaseMutationAuditId(Guid.NewGuid())) { }

    private TenantDatabaseMutationAudit(TenantDatabaseMutationAuditId id, TenantId tenantId, string idempotencyKey, DateTimeOffset now) : base(id)
    {
        TenantId = tenantId;
        IdempotencyKey = Required(idempotencyKey, nameof(idempotencyKey));
        Status = "Running";
        CreatedAtUtc = now.ToUniversalTime();
    }

    public TenantId TenantId { get; private set; } = null!;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public bool? Succeeded { get; private set; }
    public bool? Compensated { get; private set; }
    public string? ResultJson { get; private set; }
    public string? Error { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public static TenantDatabaseMutationAudit Start(TenantDatabaseMutationAuditId id, TenantId tenantId, string idempotencyKey, DateTimeOffset now) => new(id, tenantId, idempotencyKey, now);

    public void Complete(bool succeeded, bool compensated, string resultJson, string? error, DateTimeOffset now)
    {
        Succeeded = succeeded;
        Compensated = compensated;
        ResultJson = resultJson;
        Error = error;
        Status = succeeded ? "Succeeded" : compensated ? "Compensated" : "CompensationFailed";
        CompletedAtUtc = now.ToUniversalTime();
    }

    private static string Required(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 200) throw new ArgumentException("유효하지 않은 멱등성 키입니다.", name);
        return value.Trim();
    }
}
