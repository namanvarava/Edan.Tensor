using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence;

public sealed class TenantDatabaseResolver(
    PlatformDbContext db,
    ITenantDatabaseSecretResolver secretResolver) : ITenantDatabaseResolver
{
    public async Task<TenantDatabaseResolution> ResolveAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenantId);

        var binding = await db.TenantDatabaseBindings
            .AsNoTracking()
            .Where(
                x => x.TenantId == tenantId && x.Status == TenantDatabaseBindingStatus.Active)
            .OrderBy(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (binding is null)
            throw new InvalidOperationException($"활성 DB 바인딩이 없는 테넌트입니다: {tenantId.Value}");

        var credential = await secretResolver.ResolveAsync(binding.SecretKey, cancellationToken);
        var connectionString = TenantDatabaseConnectionString.Build(binding, credential);
        return new TenantDatabaseResolution(
            binding.Id,
            binding.TenantId,
            binding.Provider,
            connectionString,
            binding.SchemaName,
            binding.SecretKey);
    }

    public async Task<TenantDatabaseResolution> ResolveAsync(
        TenantDatabaseBindingId bindingId,
        CancellationToken cancellationToken = default)
    {
        var binding = await db.TenantDatabaseBindings.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == bindingId && x.Status == TenantDatabaseBindingStatus.Active, cancellationToken)
            ?? throw new InvalidOperationException($"활성 DB 바인딩이 없는 바인딩입니다: {bindingId.Value}");
        var credential = await secretResolver.ResolveAsync(binding.SecretKey, cancellationToken);
        return new TenantDatabaseResolution(binding.Id, binding.TenantId, binding.Provider,
            TenantDatabaseConnectionString.Build(binding, credential), binding.SchemaName, binding.SecretKey);
    }
}

internal static class TenantDatabaseConnectionString
{
    public static string Build(TenantDatabaseBinding binding, TenantDatabaseCredential credential)
    {
        if (binding.Provider == TenantDatabaseProvider.SQLite)
            return credential.Password;

        if (string.IsNullOrWhiteSpace(binding.ServerAddress) ||
            string.IsNullOrWhiteSpace(binding.DatabaseName))
            throw new InvalidOperationException("서버 주소와 DB 이름이 필요한 Provider입니다.");

        var sqlServerPort = binding.Port is null ? string.Empty : $",{binding.Port.Value}";
        return binding.Provider switch
        {
            TenantDatabaseProvider.PostgreSQL =>
                $"Host={binding.ServerAddress};Port={binding.Port ?? 5432};Database={binding.DatabaseName};Username={credential.Username};Password={credential.Password};Ssl Mode=Prefer",
            TenantDatabaseProvider.SqlServer =>
                $"Server={binding.ServerAddress}{sqlServerPort};Database={binding.DatabaseName};User Id={credential.Username};Password={credential.Password};TrustServerCertificate=True",
            TenantDatabaseProvider.Oracle =>
                $"User Id={credential.Username};Password={credential.Password};Data Source={binding.ServerAddress}:{binding.Port ?? 1521}/{binding.DatabaseName}",
            _ => throw new NotSupportedException($"지원하지 않는 Provider입니다: {binding.Provider}")
        };
    }
}

public sealed class EnvironmentTenantDatabaseSecretResolver(IConfiguration configuration)
    : ITenantDatabaseSecretResolver
{
    public async Task<TenantDatabaseCredential> ResolveAsync(string secretKey, CancellationToken cancellationToken = default)
    {
        var username = await ReadSecretPartAsync(
            $"TenantDatabaseSecrets:{secretKey}:UsernameFile",
            $"TenantDatabaseSecrets:{secretKey}:Username",
            cancellationToken);
        var password = await ReadSecretPartAsync(
            $"TenantDatabaseSecrets:{secretKey}:PasswordFile",
            $"TenantDatabaseSecrets:{secretKey}:Password",
            cancellationToken) ?? Environment.GetEnvironmentVariable(secretKey);
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException($"DB Secret을 찾을 수 없습니다: {secretKey}");
        return new TenantDatabaseCredential(username.Trim(), password.Trim());
    }

    private async Task<string?> ReadSecretPartAsync(string fileKey, string valueKey, CancellationToken cancellationToken)
    {
        var file = configuration[fileKey];
        if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
            return await File.ReadAllTextAsync(file, cancellationToken);
        return configuration[valueKey];
    }
}
