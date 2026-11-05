using EDAN.Tensor.DataSet.Infrastructure;
using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using EDAN.Tensor.HostServices.Services;
using EDAN.Tensor.Platform.Infrastructure.DependencyInjection;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Application.Execution;
using EDAN.Tensor.Worker.Infrastructure.DependencyInjection;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using EDAN.Tensor.WorkerHost.BackgroundServices;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Worker와 API는 동일한 Platform·DataSet·콘텐츠·Worker DB 연결을 공유해야 합니다.
var platformConnectionString = builder.Configuration.GetConnectionString("PlatformDatabase")
    ?? throw new InvalidOperationException("WorkerHost에는 API Host와 동일한 PlatformDatabase 연결 문자열이 필요합니다.");
var platformSqlite = ResolveSqliteConnection(platformConnectionString, builder.Environment.ContentRootPath);
var archivePath = builder.Configuration["AuthorizationAuditArchive:RootPath"]
    ?? "App_Data/AuthorizationAuditArchives";
if (!Path.IsPathRooted(archivePath))
    archivePath = Path.Combine(builder.Environment.ContentRootPath, archivePath);
builder.Services.AddPlatformInfrastructure(
    options => options.UseSqlite(platformSqlite),
    archivePath);

var dataSetConnectionString = builder.Configuration.GetConnectionString("DataSetDatabase")
    ?? throw new InvalidOperationException("WorkerHost에는 API Host와 동일한 DataSetDatabase 연결 문자열이 필요합니다.");
var dataSetSqlite = ResolveSqliteConnection(dataSetConnectionString, builder.Environment.ContentRootPath);
builder.Services.AddDataSetInfrastructure(options =>
    options.UseSqlite(dataSetSqlite,
        sqlite => sqlite.MigrationsAssembly(typeof(DataSetDbContext).Assembly.FullName)));

var contentConnectionString = builder.Configuration.GetConnectionString("DataSetContentDatabase")
    ?? throw new InvalidOperationException("WorkerHost에는 API Host와 동일한 DataSetContentDatabase 연결 문자열이 필요합니다.");
var contentSqlite = ResolveSqliteConnection(contentConnectionString, builder.Environment.ContentRootPath);
var maxContentBytes = builder.Configuration.GetValue<long?>("DataSetContent:MaxUploadBytes") ?? 67_108_864;
builder.Services.AddDataSetSqliteStorage(contentSqlite, maxContentBytes);

var workerConnectionString = builder.Configuration.GetConnectionString("WorkerDatabase")
    ?? builder.Configuration.GetConnectionString("Worker")
    ?? throw new InvalidOperationException("WorkerHost에는 API Host와 동일한 WorkerDatabase 연결 문자열이 필요합니다.");
var workerSqlite = ResolveSqliteConnection(workerConnectionString, builder.Environment.ContentRootPath);
builder.Services.AddWorkerInfrastructure(options =>
    options.UseSqlite(workerSqlite,
        sqlite => sqlite.MigrationsAssembly(typeof(WorkerDbContext).Assembly.FullName)));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<DataSetStorageUsageSnapshotService>();
builder.Services.AddHostedService<DataSetStorageUsageSnapshotWorker>();
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
    await migrationScope.ServiceProvider.GetRequiredService<PlatformDbContext>().Database.MigrateAsync();
    await migrationScope.ServiceProvider.GetRequiredService<WorkerDbContext>().Database.MigrateAsync();
    await migrationScope.ServiceProvider.GetRequiredService<DataSetDbContext>().Database.MigrateAsync();
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

static string ResolveSqliteConnection(string connectionString, string contentRoot)
{
    var sqlite = new SqliteConnectionStringBuilder(connectionString);
    if (!Path.IsPathRooted(sqlite.DataSource) && sqlite.DataSource != ":memory:")
    {
        sqlite.DataSource = Path.Combine(contentRoot, sqlite.DataSource);
        var directory = Path.GetDirectoryName(sqlite.DataSource);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
    }
    return sqlite.ConnectionString;
}

public partial class Program { }
