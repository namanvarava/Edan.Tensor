using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using EDAN.Tensor.DataSet.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EDAN.Tensor.DataSet.Infrastructure;

/// <summary>DataSet 메타데이터 및 콘텐츠 저장소 구현을 DI 컨테이너에 등록합니다.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataSetInfrastructure(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDbContext);
        services.AddDbContext<DataSetDbContext>(configureDbContext);
        services.AddScoped<IDataSetRepository, EfDataSetRepository>();
        return services;
    }

    /// <summary>메타데이터 DB와 별도인 SQLite 콘텐츠 DB를 등록합니다.</summary>
    public static IServiceCollection AddDataSetSqliteStorage(this IServiceCollection services, string connectionString, long maxContentBytes = 67_108_864, DataSetStorageQuotaPolicy? quotaPolicy = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        var adapter = new SqliteDataSetStorageAdapter(connectionString, maxContentBytes);
        services.AddSingleton<IDataSetStorageAdapter>(adapter);
        services.AddSingleton<IDataSetStorageUsageReader>(adapter);
        services.AddSingleton<IDataSetStorageQuotaPolicyProvider>(new FixedDataSetStorageQuotaPolicyProvider(quotaPolicy ?? new DataSetStorageQuotaPolicy(null, null, null, null)));
        services.AddSingleton<IDataSetStorageAdapterResolver, DataSetStorageAdapterResolver>();
        return services;
    }
}

internal sealed class FixedDataSetStorageQuotaPolicyProvider : IDataSetStorageQuotaPolicyProvider
{
    private readonly DataSetStorageQuotaPolicy policy;

    public FixedDataSetStorageQuotaPolicyProvider(DataSetStorageQuotaPolicy policy)
    {
        this.policy = policy ?? throw new ArgumentNullException(nameof(policy));
        if (policy.WarningThresholdPercent is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(policy), "Warning threshold must be between 1 and 100.");
        if (policy.TenantLimitBytes is < 0 || policy.OrganizationLimitBytes is < 0
            || policy.ProjectLimitBytes is < 0 || policy.DataSetLimitBytes is < 0)
            throw new ArgumentOutOfRangeException(nameof(policy), "Quota limits cannot be negative.");
    }

    public Task<DataSetStorageQuotaPolicy> GetPolicyAsync(DataSetStorageAllocation allocation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(allocation);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(policy);
    }
}
