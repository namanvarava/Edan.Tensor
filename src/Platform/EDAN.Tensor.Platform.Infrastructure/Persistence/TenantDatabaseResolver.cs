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
            .SingleOrDefaultAsync(
                x => x.TenantId == tenantId && x.Status == TenantDatabaseBindingStatus.Active,
                cancellationToken);

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

        var port = binding.Port is null ? string.Empty : $":{binding.Port.Value}";
        return binding.Provider switch
        {
            TenantDatabaseProvider.PostgreSQL =>
                $"Host={binding.ServerAddress};Port={binding.Port ?? 5432};Database={binding.DatabaseName};Username={credential.Username};Password={credential.Password};Ssl Mode=Prefer",
            TenantDatabaseProvider.SqlServer =>
                $"Server={binding.ServerAddress}{port};Database={binding.DatabaseName};User Id={credential.Username};Password={credential.Password};TrustServerCertificate=True",
            TenantDatabaseProvider.Oracle =>
                $"User Id={credential.Username};Password={credential.Password};Data Source={binding.ServerAddress}{port}/{binding.DatabaseName}",
            _ => throw new NotSupportedException($"지원하지 않는 Provider입니다: {binding.Provider}")
        };
    }
}

public sealed class EnvironmentTenantDatabaseSecretResolver(IConfiguration configuration)
    : ITenantDatabaseSecretResolver
{
    public Task<TenantDatabaseCredential> ResolveAsync(string secretKey, CancellationToken cancellationToken = default)
    {
        var username = configuration[$"TenantDatabaseSecrets:{secretKey}:Username"];
        var password = configuration[$"TenantDatabaseSecrets:{secretKey}:Password"]
            ?? Environment.GetEnvironmentVariable(secretKey);
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException($"DB Secret을 찾을 수 없습니다: {secretKey}");
        return Task.FromResult(new TenantDatabaseCredential(username, password));
    }
}
