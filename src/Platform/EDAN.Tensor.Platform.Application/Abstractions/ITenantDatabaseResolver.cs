using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.Abstractions;

public sealed record TenantDatabaseResolution(
    TenantDatabaseBindingId BindingId,
    TenantId TenantId,
    TenantDatabaseProvider Provider,
    string ConnectionString,
    string? SchemaName,
    string SecretKey);

public sealed record TenantDatabaseCredential(string Username, string Password);

/// <summary>요청의 테넌트가 사용할 활성 DB 바인딩을 해석합니다.</summary>
public interface ITenantDatabaseResolver
{
    Task<TenantDatabaseResolution> ResolveAsync(TenantId tenantId, CancellationToken cancellationToken = default);
}

/// <summary>Secret 참조 키를 실제 연결 자격증명으로 해석하는 경계입니다.</summary>
public interface ITenantDatabaseSecretResolver
{
    Task<TenantDatabaseCredential> ResolveAsync(string secretKey, CancellationToken cancellationToken = default);
}
