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
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "WorkerHost" }));
app.Run();

public partial class Program { }
