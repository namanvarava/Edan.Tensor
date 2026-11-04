using EDAN.Tensor.Platform.Infrastructure.DependencyInjection;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// SQLite는 로컬 개발용 기본 메타데이터 저장소이며 운영 환경에서는 연결 문자열을 환경 설정으로 교체합니다.
var connectionString = builder.Configuration.GetConnectionString("PlatformDatabase")
    ?? throw new InvalidOperationException("PlatformDatabase 연결 문자열이 설정되지 않았습니다.");

var databasePath = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(databasePath);

builder.Services.AddPlatformInfrastructure(options => options.UseSqlite(connectionString));
builder.Services.AddHealthChecks();

var app = builder.Build();

// 개발 환경에서는 로컬 SQLite 파일을 최초 실행 시 생성하며 운영 환경의 스키마 변경은 별도 마이그레이션으로 관리합니다.
if (app.Environment.IsDevelopment()
    && builder.Configuration.GetValue<bool>("Database:EnsureCreatedInDevelopment"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapHealthChecks("/health");

// 데이터베이스 연결 결과를 신호등 상태로 반환하며 연결 예외의 상세 정보는 외부에 노출하지 않습니다.
app.MapGet("/health/database", async (PlatformDbContext db, CancellationToken cancellationToken) =>
{
    try
    {
        var connected = await db.Database.CanConnectAsync(cancellationToken);
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
