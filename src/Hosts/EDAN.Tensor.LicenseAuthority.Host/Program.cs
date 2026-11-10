using EDAN_Tensor_Licensing_Application.Abstractions;
using EDAN_Tensor_Licensing_Application.Services;
using EDAN_Tensor_Licensing_Infrastructure.Cryptography;
using EDAN_Tensor_Licensing_Infrastructure.Persistence;
using EDAN_Tensor_Licensing_Contracts;

var builder = WebApplication.CreateBuilder(args);
var keyPath = ResolvePath(builder.Configuration["LicenseAuthority:PrivateKeyPath"], "license-authority-es256.pem");
var licensePath = ResolvePath(builder.Configuration["LicenseAuthority:DevelopmentLicensePath"], "development-license.json");
var statusPath = ResolvePath(builder.Configuration["LicenseAuthority:StatusPath"], "license-status.json");
var developmentIssuanceEnabled = builder.Environment.IsDevelopment()
    && builder.Configuration.GetValue("LicenseAuthority:EnableDevelopmentIssuance", false);

// 개인키는 저장소가 아닌 사용자 로컬 데이터 경로에 보관하며 로그·HTTP 응답에 기록하지 않습니다.
builder.Services.AddSingleton(new FileEs256KeyMaterial(
    keyPath,
    builder.Configuration["LicenseAuthority:KeyId"],
    allowGeneration: builder.Environment.IsDevelopment()));
builder.Services.AddSingleton<ILicenseTokenCodec>(services =>
    new Es256LicenseTokenCodec(services.GetRequiredService<FileEs256KeyMaterial>()));
builder.Services.AddSingleton<IDevelopmentLicenseStore>(_ => new FileDevelopmentLicenseStore(licensePath));
builder.Services.AddSingleton<ILicenseStatusStore>(_ => new FileLicenseStatusStore(statusPath));
builder.Services.AddScoped<DevelopmentLicenseService>();
builder.Services.AddScoped<LicenseValidationService>();
builder.Services.AddHealthChecks();
var app = builder.Build();
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok(new { Product = "EDAN Tensor", Service = "LicenseAuthority", Status = "Ready" }));

// 개발 발급은 Development 환경과 명시적인 플래그가 모두 있어야 하며 운영 환경에서는 경로 자체를 닫습니다.
app.MapPost("/api/licenses/development", async (
    DevelopmentLicenseService service,
    ILoggerFactory loggerFactory,
    CancellationToken cancellationToken) =>
{
    if (!developmentIssuanceEnabled)
        return Results.NotFound(new { Code = "licensing.development_issuance_disabled", Description = "개발 라이선스 발급은 명시적으로 활성화된 Development 환경에서만 사용할 수 있습니다." });

    loggerFactory.CreateLogger("LicenseAuthority").LogInformation(
        "Development 라이선스 발급을 실행합니다. Environment={Environment}, Actor={Actor}",
        app.Environment.EnvironmentName,
        Environment.UserName);
    var result = await service.IssueOrGetAsync(DateTimeOffset.UtcNow, cancellationToken);
    return result.IsSuccess
        ? Results.Ok(new DevelopmentLicenseResponse(result.Value.Token, result.Value.Descriptor))
        : Results.Conflict(new { Code = result.Error.Code, Description = result.Error.Description });
});

app.MapGet("/api/licenses/public-key", (ILicenseTokenCodec codec) =>
    Results.Ok(new LicensePublicKeyResponse(codec.KeyId, codec.PublicJwk)));

app.MapPost("/api/licenses/{licenseId:guid}/revoke", async (
    Guid licenseId,
    ILicenseStatusStore statusStore,
    CancellationToken cancellationToken) =>
{
    if (!developmentIssuanceEnabled)
        return Results.NotFound(new { Code = "licensing.development_issuance_disabled", Description = "개발 라이선스 관리는 명시적으로 활성화된 Development 환경에서만 사용할 수 있습니다." });

    await statusStore.RevokeAsync(licenseId, cancellationToken);
    return Results.Ok(new LicenseRevocationResponse(licenseId, LicenseStatus.Revoked));
});

app.MapPost("/api/licenses/validate", async (
    LicenseValidationHttpRequest request,
    LicenseValidationService service,
    CancellationToken cancellationToken) =>
{
    var result = await service.ValidateAsync(
        request.Token,
        new LicenseValidationRequest(request.SiteOrTenant, request.Product, request.RequiredFeatures),
        DateTimeOffset.UtcNow,
        cancellationToken);
    if (result.IsValid
        && !LicenseEnvironmentPolicy.IsAllowed(app.Environment.IsDevelopment(), result.Descriptor!))
    {
        return Results.Json(
            new LicenseValidationResult(
                false,
                "licensing.development_license_forbidden",
                "개발 라이선스는 운영 환경에서 사용할 수 없습니다.",
                result.Descriptor),
            statusCode: StatusCodes.Status403Forbidden);
    }

    return result.IsValid
        ? Results.Ok(result)
        : Results.Json(result, statusCode: StatusCodes.Status403Forbidden);
});

app.Run();

static string ResolvePath(string? configuredPath, string defaultFileName)
{
    var path = configuredPath;
    if (string.IsNullOrWhiteSpace(path))
    {
        var localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        path = Path.Combine(localApplicationData, "EDAN.Tensor", "LicenseAuthority", defaultFileName);
    }

    return Path.IsPathRooted(path)
        ? path
        : Path.Combine(AppContext.BaseDirectory, path);
}

/// <summary>라이선스 검증 HTTP 요청의 공개 입력을 나타냅니다.</summary>
public sealed record LicenseValidationHttpRequest(
    string Token,
    string SiteOrTenant,
    string Product,
    string[] RequiredFeatures);

public partial class Program { }
