using EDAN.Tensor.Platform.Application.DependencyInjection;
using EDAN.Tensor.ApiHost.BackgroundServices;
using EDAN.Tensor.ApiHost.Endpoints;
using EDAN.Tensor.ApiHost.Security;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Infrastructure.DependencyInjection;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using EDAN.Tensor.Platform.Infrastructure.Invitations;
using EDAN.Tensor.Worker.Application.Execution;
using EDAN.Tensor.Worker.Application.ExternalDelivery;
using EDAN.Tensor.Worker.Infrastructure.DependencyInjection;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using EDAN.Tensor.DataSet.Infrastructure;
using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Application.DependencyInjection;
using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using EDAN.Tensor.Edan.Infrastructure.Persistence;
using EDAN.Tensor.ApiHost.Services;
using EDAN.Tensor.ApiHost.Observability;
using EDAN.Tensor.HostServices.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.EntityFrameworkCore.SqlServer;
using EDAN.Tensor.PostgreSqlMigrations;
using EDAN.Tensor.SqlServerMigrations;
using EDAN.Tensor.OracleMigrations;
using Oracle.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Development 환경은 사용자 프로필 권한과 분리된 로컬 키 저장 경로를 사용합니다.
// 운영 환경은 이 분기를 실행하지 않으며 배포 환경이 관리하는 키 저장소를 사용합니다.
if (builder.Environment.IsDevelopment())
{
    var developmentKeyPath = builder.Configuration["DataProtection:DevelopmentKeyPath"]
        ?? "App_Data/DataProtection-Keys";
    if (!Path.IsPathRooted(developmentKeyPath))
        developmentKeyPath = Path.Combine(builder.Environment.ContentRootPath, developmentKeyPath);

    Directory.CreateDirectory(developmentKeyPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(developmentKeyPath))
        .SetApplicationName("EDAN.Tensor.ApiHost");
}

// SQLite는 기본 저장소로 유지하고, PostgreSQL은 환경 설정으로 선택합니다.
var databaseProvider = builder.Configuration["Database:Provider"] ?? "Sqlite";
var migrationsAssembly = GetMigrationsAssembly(databaseProvider);
var connectionString = builder.Configuration.GetConnectionString("PlatformDatabase")
    ?? throw new InvalidOperationException("PlatformDatabase 연결 문자열이 설정되지 않았습니다.");
var platformDatabaseConnection = ResolveDatabaseConnection(connectionString, databaseProvider, builder.Environment.ContentRootPath);

// SQLite 상대 경로만 API Host의 콘텐츠 루트를 기준으로 절대 경로로 변환합니다.
var sqliteConnection = IsPostgreSql(databaseProvider) || IsSqlServer(databaseProvider)
    ? null
    : new SqliteConnectionStringBuilder(connectionString);
if (sqliteConnection is not null
    && !Path.IsPathRooted(sqliteConnection.DataSource) && sqliteConnection.DataSource != ":memory:")
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
// 개발 환경은 명시된 Bypass를 사용할 수 있고, 그 밖의 환경은 외부 IdP 설정을 사용합니다.
builder.Services.AddPlatformSecurity(builder.Configuration, builder.Environment);
var archivePath = builder.Configuration["AuthorizationAuditArchive:RootPath"]
    ?? "App_Data/AuthorizationAuditArchives";
if (!Path.IsPathRooted(archivePath))
    archivePath = Path.Combine(builder.Environment.ContentRootPath, archivePath);

builder.Services.AddPlatformInfrastructure(
    options => ConfigureDatabase(options, databaseProvider, platformDatabaseConnection,
        migrationsAssembly,
        historyTable: "__EFMigrationsHistory_Platform"), archivePath);
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<DevelopmentInvitationDelivery>();
    builder.Services.AddSingleton<IInvitationDelivery>(sp => sp.GetRequiredService<DevelopmentInvitationDelivery>());
}

// Worker 작업 저장소는 별도 연결을 지원하며 PostgreSQL에서는 같은 운영 후보 DB를 공유할 수 있습니다.
var workerConnectionString = builder.Configuration.GetConnectionString("WorkerDatabase")
    ?? (IsPostgreSql(databaseProvider) || IsSqlServer(databaseProvider) ? connectionString : "Data Source=edan-worker.db");
var workerResultPath = builder.Configuration["WorkerResults:RootPath"] ?? "App_Data/WorkerResults";
if (!Path.IsPathRooted(workerResultPath))
    workerResultPath = Path.Combine(builder.Environment.ContentRootPath, workerResultPath);
Directory.CreateDirectory(workerResultPath);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddWorkerInfrastructure(options =>
    ConfigureDatabase(options, databaseProvider, ResolveDatabaseConnection(workerConnectionString, databaseProvider, builder.Environment.ContentRootPath),
        migrationsAssembly ?? typeof(WorkerDbContext).Assembly.FullName, "__EFMigrationsHistory_Worker"), workerResultPath);

// DataSet 메타데이터는 별도 연결을 지원하며 PostgreSQL에서는 같은 운영 후보 DB를 공유할 수 있습니다.
var dataSetConnectionString = builder.Configuration.GetConnectionString("DataSetDatabase")
    ?? (IsPostgreSql(databaseProvider) || IsSqlServer(databaseProvider) ? connectionString : "Data Source=App_Data/edan-tensor-dataset.db;Cache=Shared");
builder.Services.AddDataSetInfrastructure(options =>
    ConfigureDatabase(options, databaseProvider, ResolveDatabaseConnection(dataSetConnectionString, databaseProvider, builder.Environment.ContentRootPath),
        migrationsAssembly ?? typeof(DataSetDbContext).Assembly.FullName, "__EFMigrationsHistory_DataSet"));

// Edan 업무 DB는 선택적으로 활성화하며, Framework 운영 DB와 별도 연결을 사용합니다.
// 기본값은 false이므로 기존 Platform/Worker/DataSet 운영 구성을 변경하지 않습니다.
if (builder.Configuration.GetValue<bool>("Edan:Enabled"))
{
    var edanProvider = builder.Configuration["Edan:Provider"] ?? databaseProvider;
    var edanConnection = builder.Configuration.GetConnectionString("EdanDatabase")
        ?? throw new InvalidOperationException("EdanDatabase 연결 문자열이 설정되지 않았습니다.");
    builder.Services.AddDbContext<EdanDbContext>(options =>
        ConfigureDatabase(options, edanProvider,
            ResolveDatabaseConnection(edanConnection, edanProvider, builder.Environment.ContentRootPath),
            // The runtime Edan assembly owns the SQLite baseline migrations.
            // Provider-specific migrations are applied by the separate migration hosts.
            migrationsAssembly: typeof(EdanDbContext).Assembly.FullName,
            historyTable: "__EFMigrationsHistory_Edan"));
}

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
builder.Services.AddSingleton(new SqliteDatabaseHealthProbe(dataSetContentSqliteConnection.ConnectionString));
builder.Services.AddSingleton<DatabaseHealthService>();
builder.Services.AddScoped<StorageQuotaPolicyManagementService>();
builder.Services.AddScoped<DataSetStorageUsageSnapshotService>();
builder.Services.AddScoped<WorkerTaskDeadLetterService>();
builder.Services.AddScoped<WorkerTaskEnqueueService>();
builder.Services.AddScoped<WorkerTaskQueryService>();
builder.Services.AddScoped<WorkerTaskCommandService>();
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
builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var app = builder.Build();

// 처리되지 않은 예외는 공통 오류 계약으로 변환하고 내부 상세정보는 로그에만 남깁니다.
app.UseExceptionHandler();
app.UseMiddleware<RequestObservabilityMiddleware>();

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

// Development 인증 모드에 따라 개발 Tenant와 외부 주체 소속을 멱등적으로 준비합니다.
if (app.Environment.IsDevelopment())
{
    await DevelopmentBypassDataSeeder.SeedAsync(app.Services, builder.Configuration);
    await DevelopmentExternalIdentityDataSeeder.SeedAsync(app.Services, builder.Configuration);
}

// OpenAPI 문서는 개발 환경에서만 제공하여 운영 API 표면을 불필요하게 공개하지 않습니다.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "EDAN Tensor API v1");
        options.DocumentTitle = "EDAN Tensor API";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapPlatformEndpoints();
app.MapWorkerEndpoints();
app.MapDataSetEndpoints();
app.MapDataSetValidationEndpoints();
app.MapExternalDeliveryEndpoints();

// 데이터베이스 연결 결과를 신호등 상태로 반환하며 연결 예외의 상세 정보는 외부에 노출하지 않습니다.
app.MapGet("/health", async (DatabaseHealthService health, CancellationToken cancellationToken) =>
{
    var snapshot = await health.CheckAsync(cancellationToken);
    return Results.Json(new
    {
        Status = snapshot.CanConnect ? "Healthy" : "Unhealthy",
        Service = "ApiHost",
        snapshot.CanConnect,
        snapshot.Databases
    }, statusCode: snapshot.CanConnect ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable);
});

app.MapGet("/health/database", async (DatabaseHealthService health, CancellationToken cancellationToken) =>
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

app.MapGet("/health/edan", async (IServiceProvider services, CancellationToken cancellationToken) =>
{
    var edan = services.GetService<EdanDbContext>();
    if (edan is null)
    {
        return Results.Json(new
        {
            Status = "NotConfigured",
            Service = "EdanDatabase",
            CanConnect = false
        }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    var canConnect = await edan.Database.CanConnectAsync(cancellationToken);
    return Results.Json(new
    {
        Status = canConnect ? "Connected" : "Disconnected",
        Service = "EdanDatabase",
        CanConnect = canConnect
    }, statusCode: canConnect ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable);
});

app.MapGet("/", () => Results.Ok(new { Product = "EDAN Tensor", Service = "ApiHost", Status = "Foundation" }));
app.Run();

static bool IsPostgreSql(string provider) =>
    provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)
    || provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase);

static bool IsSqlServer(string provider) =>
    provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase)
    || provider.Equals("MSSQL", StringComparison.OrdinalIgnoreCase)
    || provider.Equals("SQL Server", StringComparison.OrdinalIgnoreCase);

static bool IsOracle(string provider) => provider.Equals("Oracle", StringComparison.OrdinalIgnoreCase);

static string? GetMigrationsAssembly(string provider) =>
    IsPostgreSql(provider) ? typeof(PostgreSqlMigrationsMarker).Assembly.FullName :
    IsSqlServer(provider) ? typeof(SqlServerMigrationsMarker).Assembly.FullName :
    IsOracle(provider) ? typeof(OracleMigrationsMarker).Assembly.FullName : null;

static string ResolveDatabaseConnection(string connectionString, string provider, string contentRoot)
{
    if (IsPostgreSql(provider) || IsSqlServer(provider) || IsOracle(provider))
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

    if (IsOracle(provider))
    {
        options.UseOracle(connectionString, oracle =>
        {
            if (!string.IsNullOrWhiteSpace(migrationsAssembly)) oracle.MigrationsAssembly(migrationsAssembly);
            if (!string.IsNullOrWhiteSpace(historyTable)) oracle.MigrationsHistoryTable(historyTable);
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
