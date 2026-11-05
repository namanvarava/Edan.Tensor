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
builder.Services.AddSingleton<WorkerExecutionRegistry>();
builder.Services.AddHostedService<FoundationWorker>();

var app = builder.Build();
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "WorkerHost" }));
app.Run();

internal sealed class FoundationWorker(
    ILogger<FoundationWorker> logger,
    WorkerExecutionRegistry executions,
    IConfiguration configuration) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("EDAN Tensor WorkerHost foundation started.");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var graceSeconds = Math.Clamp(configuration.GetValue("Worker:ShutdownGraceSeconds", 30), 1, 300);
        var handoffSeconds = Math.Clamp(configuration.GetValue("Worker:ShutdownHandoffSeconds", 15), 1, 120);
        var result = await executions.StopAsync(
            TimeSpan.FromSeconds(graceSeconds),
            TimeSpan.FromSeconds(handoffSeconds),
            cancellationToken);

        logger.LogInformation(
            "Worker 종료 조정 완료: 유예 시간 내 정리={Drained}, 인계 요청={Requested}, 인계 성공={Succeeded}, 실행 잔여={Remaining}",
            result.DrainedWithinGracePeriod,
            result.HandoffRequested,
            result.HandoffSucceeded,
            result.StillRunning);

        await base.StopAsync(cancellationToken);
    }
}

public partial class Program { }
