var builder=WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<FoundationWorker>();
var app=builder.Build();
app.MapGet("/health",()=>Results.Ok(new { Status="Healthy", Service="WorkerHost" }));
app.Run();
internal sealed class FoundationWorker(ILogger<FoundationWorker> logger) : BackgroundService
{
 protected override Task ExecuteAsync(CancellationToken stoppingToken) { logger.LogInformation("EDAN Tensor WorkerHost foundation started."); return Task.CompletedTask; }
}
public partial class Program;
