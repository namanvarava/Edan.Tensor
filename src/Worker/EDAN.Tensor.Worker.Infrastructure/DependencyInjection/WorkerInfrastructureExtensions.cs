using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Infrastructure.Execution;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EDAN.Tensor.Worker.Infrastructure.DependencyInjection;

/// <summary>전용 Worker 저장소와 작업 저장소 구현을 등록합니다.</summary>
public static class WorkerInfrastructureExtensions
{
    public static IServiceCollection AddWorkerInfrastructure(
        this IServiceCollection services, Action<DbContextOptionsBuilder> configureDatabase)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDatabase);
        services.AddDbContext<WorkerDbContext>(configureDatabase);
        services.AddScoped<IWorkerTaskRepository, WorkerTaskRepository>();
        services.AddSingleton<IWorkerTaskLeaseRenewer, WorkerTaskLeaseRenewer>();
        return services;
    }
}
