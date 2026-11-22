using EDAN.Tensor.DataSet.Infrastructure;
using EDAN.Tensor.DataSet.Application.DependencyInjection;
using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Application.Validation;
using EDAN.Tensor.Worker.Application.ExternalDelivery;
using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using EDAN.Tensor.HostServices.Services;
using EDAN.Tensor.Platform.Infrastructure.DependencyInjection;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Application.Execution;
using EDAN.Tensor.WorkerHost.Handlers;
using EDAN.Tensor.Worker.Infrastructure.DependencyInjection;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using EDAN.Tensor.WorkerHost.BackgroundServices;
using EDAN.Tensor.WorkerHost.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.EntityFrameworkCore.SqlServer;
using EDAN.Tensor.PostgreSqlMigrations;
using EDAN.Tensor.SqlServerMigrations;

var builder = WebApplication.CreateBuilder(args);

// Worker와 API는 동일한 Platform·DataSet·콘텐츠·Worker DB 연결을 공유해야 합니다.
var platformConnectionString = builder.Configuration.GetConnectionString("PlatformDatabase")
    ?? throw new InvalidOperationException("WorkerHost에는 API Host와 동일한 PlatformDatabase 연결 문자열이 필요합니다.");
var databaseProvider = builder.Configuration["Database:Provider"] ?? "Sqlite";
var migrationsAssembly = GetMigrationsAssembly(databaseProvider);
var archivePath = builder.Configuration["AuthorizationAuditArchive:RootPath"]
    ?? "App_Data/AuthorizationAuditArchives";
if (!Path.IsPathRooted(archivePath))
    archivePath = Path.Combine(builder.Environment.ContentRootPath, archivePath);
builder.Services.AddPlatformInfrastructure(
    options => ConfigureDatabase(options, databaseProvider, ResolveDatabaseConnection(platformConnectionString, databaseProvider, builder.Environment.ContentRootPath),
        migrationsAssembly,
        historyTable: "__EFMigrationsHistory_Platform"),
    archivePath);

var dataSetConnectionString = builder.Configuration.GetConnectionString("DataSetDatabase")
    ?? (IsPostgreSql(databaseProvider) || IsSqlServer(databaseProvider) ? platformConnectionString : throw new InvalidOperationException("WorkerHost에는 API Host와 동일한 DataSetDatabase 연결 문자열이 필요합니다."));
builder.Services.AddDataSetInfrastructure(options =>
    ConfigureDatabase(options, databaseProvider, ResolveDatabaseConnection(dataSetConnectionString, databaseProvider, builder.Environment.ContentRootPath),
        migrationsAssembly ?? typeof(DataSetDbContext).Assembly.FullName, "__EFMigrationsHistory_DataSet"));
builder.Services.AddDataSetApplication();
builder.Services.AddScoped<IDataSetScopeValidator, WorkerDataSetScopeValidator>();

var contentConnectionString = builder.Configuration.GetConnectionString("DataSetContentDatabase")
    ?? throw new InvalidOperationException("WorkerHost에는 API Host와 동일한 DataSetContentDatabase 연결 문자열이 필요합니다.");
var contentSqlite = ResolveDatabaseConnection(contentConnectionString, "Sqlite", builder.Environment.ContentRootPath);
var maxContentBytes = builder.Configuration.GetValue<long?>("DataSetContent:MaxUploadBytes") ?? 67_108_864;
builder.Services.AddDataSetSqliteStorage(contentSqlite, maxContentBytes);
builder.Services.AddSingleton(new SqliteDatabaseHealthProbe(contentSqlite));
builder.Services.AddSingleton<WorkerDatabaseHealthService>();

var workerConnectionString = builder.Configuration.GetConnectionString("WorkerDatabase")
    ?? builder.Configuration.GetConnectionString("Worker")
    ?? (IsPostgreSql(databaseProvider) || IsSqlServer(databaseProvider) ? platformConnectionString : throw new InvalidOperationException("WorkerHost에는 API Host와 동일한 WorkerDatabase 연결 문자열이 필요합니다."));
builder.Services.AddWorkerInfrastructure(options =>
    ConfigureDatabase(options, databaseProvider, ResolveDatabaseConnection(workerConnectionString, databaseProvider, builder.Environment.ContentRootPath),
        migrationsAssembly ?? typeof(WorkerDbContext).Assembly.FullName, "__EFMigrationsHistory_Worker"));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<DataSetStorageUsageSnapshotService>();
builder.Services.AddScoped<WorkerTaskEnqueueService>();
var deliveryHosts = builder.Configuration.GetSection("ExternalDelivery:AllowedHosts")
    .GetChildren().Select(section => section.Value).OfType<string>()
    .ToHashSet(StringComparer.OrdinalIgnoreCase);
var deliveryTimeoutSeconds = Math.Clamp(
    builder.Configuration.GetValue<int?>("ExternalDelivery:TimeoutSeconds") ?? 30, 1, 300);
builder.Services.AddSingleton(ExternalDeliveryOptions.Create(deliveryHosts,
    builder.Configuration.GetValue("ExternalDelivery:AllowLoopback", false),
    TimeSpan.FromSeconds(deliveryTimeoutSeconds), builder.Environment.IsDevelopment()));
builder.Services.AddHttpClient<ExternalDeliveryService>((serviceProvider, client) =>
{
    client.Timeout = serviceProvider.GetRequiredService<ExternalDeliveryOptions>().Timeout;
});
builder.Services.AddWorkerTaskHandler<DataSetStorageUsageSnapshotTaskHandler>(DataSetStorageUsageSnapshotWorker.TaskType);
builder.Services.AddWorkerTaskHandler<DataSetValidationTaskHandler>(DataSetValidationTaskContract.TaskType);
builder.Services.AddWorkerTaskHandler<ExternalDeliveryTaskHandler>(ExternalDeliveryTaskContract.TaskType);
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
app.MapGet("/health/database", async (WorkerDatabaseHealthService health, CancellationToken cancellationToken) =>
{
    var snapshot = await health.CheckAsync(cancellationToken);
    return Results.Json(new
    {
        snapshot.Status,
        snapshot.Indicator,
        snapshot.CanConnect,
        snapshot.Databases
    }, statusCode: snapshot.CanConnect ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable);
});
app.Run();

static bool IsPostgreSql(string provider) =>
    provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)
    || provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase);

static bool IsSqlServer(string provider) =>
    provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase)
    || provider.Equals("MSSQL", StringComparison.OrdinalIgnoreCase)
    || provider.Equals("SQL Server", StringComparison.OrdinalIgnoreCase);

static string? GetMigrationsAssembly(string provider) =>
    IsPostgreSql(provider) ? typeof(PostgreSqlMigrationsMarker).Assembly.FullName :
    IsSqlServer(provider) ? typeof(SqlServerMigrationsMarker).Assembly.FullName : null;

static string ResolveDatabaseConnection(string connectionString, string provider, string contentRoot)
{
    if (IsPostgreSql(provider) || IsSqlServer(provider))
        return connectionString;

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

static DbContextOptionsBuilder ConfigureDatabase(
    DbContextOptionsBuilder options,
    string provider,
    string connectionString,
    string? migrationsAssembly = null,
    string? historyTable = null)
{
    if (IsPostgreSql(provider))
    {
        options.UseNpgsql(connectionString, npgsql =>
        {
            if (!string.IsNullOrWhiteSpace(migrationsAssembly))
                npgsql.MigrationsAssembly(migrationsAssembly);
            if (!string.IsNullOrWhiteSpace(historyTable))
                npgsql.MigrationsHistoryTable(historyTable);
        });
        return options;
    }

    if (IsSqlServer(provider))
    {
        options.UseSqlServer(connectionString, sqlServer =>
        {
            if (!string.IsNullOrWhiteSpace(migrationsAssembly))
                sqlServer.MigrationsAssembly(migrationsAssembly);
            if (!string.IsNullOrWhiteSpace(historyTable))
                sqlServer.MigrationsHistoryTable(historyTable);
        });
        return options;
    }

    options.UseSqlite(connectionString, sqlite =>
    {
        if (!string.IsNullOrWhiteSpace(migrationsAssembly))
            sqlite.MigrationsAssembly(migrationsAssembly);
    });
    return options;
}

public partial class Program { }
