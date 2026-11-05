using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EDAN.Tensor.DataSet.Infrastructure;

/// <summary>DataSet 메타데이터 저장소 구현을 DI 컨테이너에 등록합니다.</summary>
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
}
