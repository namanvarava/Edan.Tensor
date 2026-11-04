using EDAN.Tensor.Platform.Application.DependencyInjection;
using EDAN.Tensor.ApiHost.Endpoints;
using EDAN.Tensor.Platform.Infrastructure.DependencyInjection;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
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
builder.Services.AddPlatformInfrastructure(options => options.UseSqlite(sqliteConnection.ConnectionString));
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
}

// OpenAPI 문서는 개발 환경에서만 제공하여 운영 API 표면을 불필요하게 공개하지 않습니다.
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapHealthChecks("/health");
app.MapPlatformEndpoints();

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
