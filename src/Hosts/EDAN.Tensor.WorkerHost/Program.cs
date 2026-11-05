using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Application.Execution;
using EDAN.Tensor.Worker.Infrastructure.DependencyInjection;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var workerConnectionString = builder.Configuration.GetConnectionString("Worker")
    ?? "Data Source=edan-worker.db";
builder.Services.AddWorkerInfrastructure(options =>
    options.UseSqlite(workerConnectionString,
        sqlite => sqlite.MigrationsAssembly(typeof(WorkerDbContext).Assembly.FullName)));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<WorkerExecutionRegistry>();
// 업무별 Handler는 배포 구성에서 AddWorkerTaskHandler<THandler>(taskType)로 등록합니다.
builder.Services.AddWorkerTaskHandlerRegistry();
builder.Services.AddScoped<WorkerTaskExecutionCoordinator>();
builder.Services.AddHostedService<WorkerTaskDispatcher>();

var app = builder.Build();

// 운영 환경에서는 배포 단계에서 Migration을 적용하고, 개발 환경에서는 설정에 따라 자동 적용합니다.
if (app.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("Database:ApplyMigrationsInDevelopment"))
{
    await using var migrationScope = app.Services.CreateAsyncScope();
    var migrationDb = migrationScope.ServiceProvider.GetRequiredService<WorkerDbContext>();
    await migrationDb.Database.MigrateAsync();
}

// 실제 Handler 등록을 Worker DB 카탈로그에 멱등적으로 동기화합니다.
await using (var catalogScope = app.Services.CreateAsyncScope())
{
    var registry = catalogScope.ServiceProvider.GetRequiredService<WorkerTaskHandlerRegistry>();
    var catalog = catalogScope.ServiceProvider.GetRequiredService<IWorkerTaskTypeCatalog>();
    await catalog.SynchronizeAsync(registry.GetTaskTypes(),
        catalogScope.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow());
}
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "WorkerHost" }));
app.Run();

public partial class Program { }
