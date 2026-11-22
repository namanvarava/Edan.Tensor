using EDAN.Tensor.SharedKernel.Primitives;

namespace EDAN.Tensor.Platform.Domain.Model;

public sealed record TenantDatabaseBindingId : StronglyTypedId<Guid>
{
    public TenantDatabaseBindingId(Guid value) : base(value == Guid.Empty
        ? throw new ArgumentException("DB 바인딩 식별자는 비어 있을 수 없습니다.")
        : value) { }
}

public enum TenantDatabaseProvider
{
    SQLite,
    PostgreSQL,
    SqlServer,
    Oracle
}

public enum TenantDatabaseBindingStatus
{
    Provisioning,
    Active,
    Suspended,
    Failed
}

/// <summary>테넌트가 사용할 외부 업무 DB와 Provider의 바인딩을 관리합니다.</summary>
public sealed class TenantDatabaseBinding : AggregateRoot<TenantDatabaseBindingId>
{
    private TenantDatabaseBinding(
        TenantDatabaseBindingId id,
        TenantId tenantId,
        TenantDatabaseProvider provider,
        string username,
        string secretKey,
        string? serverAddress,
        int? port,
        string? databaseName,
        string? schemaName,
        DateTimeOffset createdAtUtc) : base(id)
    {
        if (!Enum.IsDefined(provider)) throw new ArgumentOutOfRangeException(nameof(provider));
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("DB 사용자 이름이 필요합니다.", nameof(username));
        if (string.IsNullOrWhiteSpace(secretKey)) throw new ArgumentException("DB Secret 참조 키가 필요합니다.", nameof(secretKey));
        if (port is <= 0 or > 65535) throw new ArgumentOutOfRangeException(nameof(port));
        TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
        Provider = provider;
        Username = username.Trim();
        SecretKey = secretKey.Trim();
        ServerAddress = Normalize(serverAddress);
        Port = port;
        DatabaseName = Normalize(databaseName);
        SchemaName = Normalize(schemaName);
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        Status = TenantDatabaseBindingStatus.Provisioning;
    }

    public TenantId TenantId { get; }
    public TenantDatabaseProvider Provider { get; }
    public string Username { get; private set; }
    public string SecretKey { get; private set; }
    public string? ServerAddress { get; private set; }
    public int? Port { get; private set; }
    public string? DatabaseName { get; private set; }
    public string? SchemaName { get; private set; }
    public TenantDatabaseBindingStatus Status { get; private set; }
    public string? MigrationVersion { get; private set; }
    public DateTimeOffset? LastHealthAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public static TenantDatabaseBinding Create(
        TenantDatabaseBindingId id,
        TenantId tenantId,
        TenantDatabaseProvider provider,
        string username,
        string secretKey,
        string? serverAddress,
        int? port,
        string? databaseName,
        string? schemaName,
        DateTimeOffset createdAtUtc) => new(id, tenantId, provider, username, secretKey, serverAddress, port, databaseName, schemaName, createdAtUtc);

    public void Activate(DateTimeOffset nowUtc, string? migrationVersion = null)
    {
        if (Status is TenantDatabaseBindingStatus.Failed or TenantDatabaseBindingStatus.Suspended)
            throw new InvalidOperationException("현재 DB 바인딩 상태에서는 활성화할 수 없습니다.");
        Status = TenantDatabaseBindingStatus.Active;
        MigrationVersion = migrationVersion?.Trim();
        UpdatedAtUtc = nowUtc.ToUniversalTime();
    }

    public void MarkFailed(DateTimeOffset nowUtc)
    {
        Status = TenantDatabaseBindingStatus.Failed;
        UpdatedAtUtc = nowUtc.ToUniversalTime();
    }

    public void Suspend(DateTimeOffset nowUtc)
    {
        if (Status != TenantDatabaseBindingStatus.Active)
            throw new InvalidOperationException("활성 상태의 DB 바인딩만 중지할 수 있습니다.");
        Status = TenantDatabaseBindingStatus.Suspended;
        UpdatedAtUtc = nowUtc.ToUniversalTime();
    }

    public void RecordHealth(DateTimeOffset nowUtc, string? migrationVersion = null)
    {
        LastHealthAtUtc = nowUtc.ToUniversalTime();
        MigrationVersion = migrationVersion?.Trim() ?? MigrationVersion;
        UpdatedAtUtc = LastHealthAtUtc;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
