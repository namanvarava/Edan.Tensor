using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Application.Execution;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        services.AddScoped<WorkerTaskRepository>();
        services.AddScoped<IWorkerTaskRepository>(sp => sp.GetRequiredService<WorkerTaskRepository>());
        services.AddScoped<IWorkerTaskDeadLetterRepository>(sp => sp.GetRequiredService<WorkerTaskRepository>());
        services.AddSingleton<IWorkerTaskLeaseRenewer, WorkerTaskLeaseRenewer>();
        services.AddScoped<WorkerTaskProgressReporter>();\n        services.AddScoped<WorkerTaskProgressReporter>();
        return services;
    }

    /// <summary>등록된 Handler 구성으로 작업 유형 레지스트리와 Resolver를 구성합니다.</summary>
    public static IServiceCollection AddWorkerTaskHandlerRegistry(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<WorkerTaskHandlerRegistry>(sp =>
            new WorkerTaskHandlerRegistry(sp.GetServices<WorkerTaskHandlerRegistration>()));
        services.TryAddSingleton<IWorkerTaskHandlerResolver>(sp =>
            sp.GetRequiredService<WorkerTaskHandlerRegistry>());
        return services;
    }

    /// <summary>작업 유형별 Singleton Handler를 등록합니다. 실제 업무 Handler는 WorkerHost 조합 지점에서 추가합니다.</summary>
    public static IServiceCollection AddWorkerTaskHandler<THandler>(
        this IServiceCollection services, string taskType)
        where THandler : class, IWorkerTaskHandler
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(taskType);
        services.AddSingleton<THandler>();
        services.AddSingleton(sp => new WorkerTaskHandlerRegistration(
            taskType.Trim(), sp.GetRequiredService<THandler>()));
        return services.AddWorkerTaskHandlerRegistry();
    }
}
