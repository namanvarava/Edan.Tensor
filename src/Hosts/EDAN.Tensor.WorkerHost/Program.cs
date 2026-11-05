using EDAN.Tensor.Worker.Infrastructure.DependencyInjection;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var workerConnectionString = builder.Configuration.GetConnectionString("Worker")
    ?? "Data Source=edan-worker.db";
builder.Services.AddWorkerInfrastructure(options =>
    options.UseSqlite(workerConnectionString,
        sqlite => sqlite.MigrationsAssembly(typeof(WorkerDbContext).Assembly.FullName)));
builder.Services.AddHostedService<FoundationWorker>();

var app = builder.Build();
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "WorkerHost" }));
app.Run();

internal sealed class FoundationWorker(ILogger<FoundationWorker> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("EDAN Tensor WorkerHost foundation started.");
        return Task.CompletedTask;
    }
}

public partial class Program { }
