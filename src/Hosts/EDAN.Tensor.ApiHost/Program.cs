using EDAN.Tensor.Platform.Application.DependencyInjection;
using EDAN.Tensor.ApiHost.BackgroundServices;
using EDAN.Tensor.ApiHost.Endpoints;
using EDAN.Tensor.ApiHost.Security;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.Platform.Infrastructure.DependencyInjection;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using EDAN.Tensor.Worker.Application.Execution;
using EDAN.Tensor.Worker.Infrastructure.DependencyInjection;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using EDAN.Tensor.DataSet.Infrastructure;
using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Application.DependencyInjection;
using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using EDAN.Tensor.ApiHost.Services;
using EDAN.Tensor.Platform.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// SQLite는 로컬 개발용 기본 메타데이터 저장소이며 운영 환경에서는 연결 문자열을 환경 설정으로 교체합니다.
var connectionString = builder.Configuration.GetConnectionString("PlatformDatabase")
    ?? throw new InvalidOperationException("PlatformDatabase 연결 문자열이 설정되지 않았습니다.");

// 상대 경로는 API Host의 콘텐츠 루트를 기준으로 절대 경로로 변환합니다.
var sqliteConnection = new SqliteConnectionStringBuilder(connectionString);
if (!Path.IsPathRooted(sqliteConnection.DataSource) && sqliteConnection.DataSource != ":memory:")
{
    sqliteConnection.DataSource = Path.Combine(builder.Environment.ContentRootPath, sqliteConnection.DataSource);
    var databaseDirectory = Path.GetDirectoryName(sqliteConnection.DataSource);
    if (!string.IsNullOrWhiteSpace(databaseDirectory))
        Directory.CreateDirectory(databaseDirectory);
}

builder.Services.AddPlatformApplication();
builder.Services.AddDataSetApplication();
builder.Services.AddScoped<IDataSetScopeValidator, PlatformDataSetScopeValidator>();
builder.Services.AddHostedService<AuthorizationAuditArchiveSchedulerWorker>();
builder.Services.AddPlatformSecurity(builder.Configuration);
var archivePath = builder.Configuration["AuthorizationAuditArchive:RootPath"]
    ?? "App_Data/AuthorizationAuditArchives";
if (!Path.IsPathRooted(archivePath))
    archivePath = Path.Combine(builder.Environment.ContentRootPath, archivePath);

builder.Services.AddPlatformInfrastructure(
    options => options.UseSqlite(sqliteConnection.ConnectionString), archivePath);

// Worker 작업 저장소는 Platform 메타데이터와 분리된 전용 연결을 사용합니다.
var workerConnectionString = builder.Configuration.GetConnectionString("WorkerDatabase")
    ?? "Data Source=edan-worker.db";
var workerSqliteConnection = new SqliteConnectionStringBuilder(workerConnectionString);
if (!Path.IsPathRooted(workerSqliteConnection.DataSource) && workerSqliteConnection.DataSource != ":memory:")
{
    workerSqliteConnection.DataSource = Path.Combine(builder.Environment.ContentRootPath, workerSqliteConnection.DataSource);
    var workerDirectory = Path.GetDirectoryName(workerSqliteConnection.DataSource);
    if (!string.IsNullOrWhiteSpace(workerDirectory))
        Directory.CreateDirectory(workerDirectory);
}
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddWorkerInfrastructure(options =>
    options.UseSqlite(workerSqliteConnection.ConnectionString,
        sqlite => sqlite.MigrationsAssembly(typeof(WorkerDbContext).Assembly.FullName)));

// DataSet 메타데이터는 Platform·Worker와 분리된 전용 SQLite 데이터베이스에 저장합니다.
var dataSetConnectionString = builder.Configuration.GetConnectionString("DataSetDatabase")
    ?? "Data Source=App_Data/edan-tensor-dataset.db;Cache=Shared";
var dataSetSqliteConnection = new SqliteConnectionStringBuilder(dataSetConnectionString);
if (!Path.IsPathRooted(dataSetSqliteConnection.DataSource) && dataSetSqliteConnection.DataSource != ":memory:")
{
    dataSetSqliteConnection.DataSource = Path.Combine(builder.Environment.ContentRootPath, dataSetSqliteConnection.DataSource);
    var dataSetDirectory = Path.GetDirectoryName(dataSetSqliteConnection.DataSource);
    if (!string.IsNullOrWhiteSpace(dataSetDirectory))
        Directory.CreateDirectory(dataSetDirectory);
}
builder.Services.AddDataSetInfrastructure(options =>
    options.UseSqlite(dataSetSqliteConnection.ConnectionString,
        sqlite => sqlite.MigrationsAssembly(typeof(DataSetDbContext).Assembly.FullName)));

// 콘텐츠 본문은 DataSet 메타데이터와 분리된 전용 SQLite DB에 저장합니다.
var dataSetContentConnectionString = builder.Configuration.GetConnectionString("DataSetContentDatabase")
    ?? "Data Source=App_Data/edan-tensor-dataset-content.db;Cache=Shared";
var dataSetContentSqliteConnection = new SqliteConnectionStringBuilder(dataSetContentConnectionString);
if (!Path.IsPathRooted(dataSetContentSqliteConnection.DataSource) && dataSetContentSqliteConnection.DataSource != ":memory:")
{
    dataSetContentSqliteConnection.DataSource = Path.Combine(builder.Environment.ContentRootPath, dataSetContentSqliteConnection.DataSource);
    var contentDirectory = Path.GetDirectoryName(dataSetContentSqliteConnection.DataSource);
    if (!string.IsNullOrWhiteSpace(contentDirectory))
        Directory.CreateDirectory(contentDirectory);
}
var maxDataSetContentBytes = builder.Configuration.GetValue<long?>("DataSetContent:MaxUploadBytes") ?? 67_108_864;
var dataSetQuotaPolicy = new DataSetStorageQuotaPolicy(
    builder.Configuration.GetValue<long?>("DataSetContent:Quota:TenantLimitBytes"),
    builder.Configuration.GetValue<long?>("DataSetContent:Quota:OrganizationLimitBytes"),
    builder.Configuration.GetValue<long?>("DataSetContent:Quota:ProjectLimitBytes"),
    builder.Configuration.GetValue<long?>("DataSetContent:Quota:DataSetLimitBytes"),
    builder.Configuration.GetValue<int?>("DataSetContent:Quota:WarningThresholdPercent") ?? 80);
var quotaAssignments = builder.Configuration.GetSection("DataSetContent:Quota:Assignments")
    .GetChildren()
    .Select(section => new DataSetStorageQuotaPolicyAssignment(
        Enum.Parse<DataSetStorageQuotaScope>(section["Scope"] ?? throw new InvalidOperationException("Quota assignment Scope is required."), ignoreCase: true),
        Guid.Parse(section["TargetId"] ?? throw new InvalidOperationException("Quota assignment TargetId is required.")),
        new DataSetStorageQuotaPolicy(
            section.GetValue<long?>("TenantLimitBytes"),
            section.GetValue<long?>("OrganizationLimitBytes"),
            section.GetValue<long?>("ProjectLimitBytes"),
            section.GetValue<long?>("DataSetLimitBytes"),
            section.GetValue<int?>("WarningThresholdPercent") ?? dataSetQuotaPolicy.WarningThresholdPercent)))
    .ToArray();
builder.Services.AddDataSetSqliteStorage(dataSetContentSqliteConnection.ConnectionString, maxDataSetContentBytes, dataSetQuotaPolicy, quotaAssignments);
builder.Services.AddScoped<IDataSetStorageQuotaPolicyProvider, PlatformDataSetStorageQuotaPolicyProvider>();
builder.Services.AddScoped<WorkerTaskDeadLetterService>();
builder.Services.AddScoped<WorkerTaskEnqueueService>();
builder.Services.AddScoped<WorkerTaskQueryService>();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

var app = builder.Build();

// 개발 환경에서는 버전이 관리되는 EF Core Migration을 적용하고 운영 환경의 스키마 변경은 배포 절차에서 수행합니다.
if (app.Environment.IsDevelopment()
    && builder.Configuration.GetValue<bool>("Database:ApplyMigrationsInDevelopment"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    await db.Database.MigrateAsync();
    var workerDb = scope.ServiceProvider.GetRequiredService<WorkerDbContext>();
    await workerDb.Database.MigrateAsync();
    var dataSetDb = scope.ServiceProvider.GetRequiredService<DataSetDbContext>();
    await dataSetDb.Database.MigrateAsync();
}

// OpenAPI 문서는 개발 환경에서만 제공하여 운영 API 표면을 불필요하게 공개하지 않습니다.
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapPlatformEndpoints();
app.MapWorkerEndpoints();
app.MapDataSetEndpoints();

// 데이터베이스 연결 결과를 신호등 상태로 반환하며 연결 예외의 상세 정보는 외부에 노출하지 않습니다.
app.MapGet("/health/database", async (PlatformDbContext db, CancellationToken cancellationToken) =>
{
    try
    {
        var connected = await db.Database.CanConnectAsync(cancellationToken);
        if (connected)
            _ = await db.Tenants.AnyAsync(cancellationToken);
        return Results.Ok(new
        {
            Status = connected ? "Connected" : "Disconnected",
            Indicator = connected ? "Green" : "Red",
            CanConnect = connected
        });
    }
    catch
    {
        return Results.Json(new
        {
            Status = "Disconnected",
            Indicator = "Red",
            CanConnect = false
        }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapGet("/", () => Results.Ok(new { Product = "EDAN Tensor", Service = "ApiHost", Status = "Foundation" }));
app.Run();

public partial class Program { }
