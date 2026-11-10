using System.Net;
using System.Net.Http.Json;
using System.Text;
using EDAN_Tensor_Licensing_Application.Abstractions;
using EDAN_Tensor_Licensing_Contracts;
using EDAN_Tensor_Licensing_Infrastructure.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace EDAN_Tensor_Licensing_Tests;

/// <summary>LicenseAuthority의 실제 HTTP 계약이 승인된 개발 라이선스 경계를 유지하는지 검증합니다.</summary>
public sealed class LicenseAuthorityHttpTests
{
    /// <summary>발급·공개키·정상·위조·기간·Tenant·KeyId·폐기·운영 차단을 HTTP 경계에서 검증합니다.</summary>
    [Fact]
    public async Task LicenseAuthorityHttpApi_EnforcesApprovedDevelopmentLicenseMatrix()
    {
        var root = Path.Combine(Path.GetTempPath(), $"edan-license-http-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            using var developmentFactory = new LicenseAuthorityFactory(root, "Development");
            using var developmentClient = developmentFactory.CreateClient();

            var firstIssueResponse = await developmentClient.PostAsync("/api/licenses/development", content: null);
            var firstIssue = await ReadAsync<DevelopmentLicenseResponse>(firstIssueResponse, HttpStatusCode.OK);
            var secondIssueResponse = await developmentClient.PostAsync("/api/licenses/development", content: null);
            var secondIssue = await ReadAsync<DevelopmentLicenseResponse>(secondIssueResponse, HttpStatusCode.OK);

            Assert.Equal(firstIssue.Token, secondIssue.Token);
            Assert.Equal(firstIssue.Descriptor.LicenseId, secondIssue.Descriptor.LicenseId);
            Assert.NotEqual(Guid.Empty, firstIssue.Descriptor.LicenseId);
            Assert.Equal(DevelopmentLicenseConstants.TenantCode, firstIssue.Descriptor.SiteOrTenant);
            Assert.Equal(DevelopmentLicenseConstants.Product, firstIssue.Descriptor.Product);
            Assert.Equal(DevelopmentLicenseConstants.Features, firstIssue.Descriptor.Features);
            Assert.Equal(firstIssue.Descriptor.StartsAtUtc.AddYears(5), firstIssue.Descriptor.ExpiresAtUtc);

            var publicKeyResponse = await developmentClient.GetAsync("/api/licenses/public-key");
            var publicKey = await ReadAsync<LicensePublicKeyResponse>(publicKeyResponse, HttpStatusCode.OK);
            Assert.Equal(firstIssue.Descriptor.KeyId, publicKey.KeyId);
            Assert.Contains("\"alg\":\"ES256\"", publicKey.Jwk, StringComparison.Ordinal);
            Assert.DoesNotContain("\"d\"", publicKey.Jwk, StringComparison.Ordinal);

            var normal = await ValidateAsync(
                developmentClient,
                firstIssue.Token,
                DevelopmentLicenseConstants.TenantCode,
                [DevelopmentLicenseConstants.PlatformFeature],
                HttpStatusCode.OK);
            Assert.True(normal.IsValid);
            Assert.Equal(firstIssue.Descriptor.LicenseId, normal.Descriptor!.LicenseId);

            var tampered = await ValidateAsync(
                developmentClient,
                TamperSignature(firstIssue.Token),
                DevelopmentLicenseConstants.TenantCode,
                [DevelopmentLicenseConstants.PlatformFeature],
                HttpStatusCode.Forbidden);
            Assert.False(tampered.IsValid);
            Assert.Equal("licensing.token.signature_invalid", tampered.ErrorCode);

            var otherTenant = await ValidateAsync(
                developmentClient,
                firstIssue.Token,
                "OTHER-TENANT",
                [DevelopmentLicenseConstants.PlatformFeature],
                HttpStatusCode.Forbidden);
            Assert.False(otherTenant.IsValid);
            Assert.Equal("licensing.tenant_mismatch", otherTenant.ErrorCode);

            var codec = developmentFactory.Services.GetRequiredService<ILicenseTokenCodec>();
            var now = DateTimeOffset.UtcNow;
            var expired = await ValidateAsync(
                developmentClient,
                codec.CreateToken(CreateDescriptor(codec, now.AddYears(-6), now.AddMinutes(-1))),
                DevelopmentLicenseConstants.TenantCode,
                [DevelopmentLicenseConstants.DataSetFeature],
                HttpStatusCode.Forbidden);
            Assert.False(expired.IsValid);
            Assert.Equal("licensing.expired", expired.ErrorCode);

            var notYetValid = await ValidateAsync(
                developmentClient,
                codec.CreateToken(CreateDescriptor(codec, now.AddMinutes(5), now.AddYears(5))),
                DevelopmentLicenseConstants.TenantCode,
                [DevelopmentLicenseConstants.WorkerFeature],
                HttpStatusCode.Forbidden);
            Assert.False(notYetValid.IsValid);
            Assert.Equal("licensing.not_yet_valid", notYetValid.ErrorCode);

            var unknownKeyId = await ValidateAsync(
                developmentClient,
                ReplaceKeyId(firstIssue.Token, "unknown-http-key"),
                DevelopmentLicenseConstants.TenantCode,
                [DevelopmentLicenseConstants.PlatformFeature],
                HttpStatusCode.Forbidden);
            Assert.False(unknownKeyId.IsValid);
            Assert.Equal("licensing.token.key_invalid", unknownKeyId.ErrorCode);

            using var productionFactory = new LicenseAuthorityFactory(root, "Production");
            using var productionClient = productionFactory.CreateClient();
            var productionIssuance = await productionClient.PostAsync("/api/licenses/development", content: null);
            Assert.Equal(HttpStatusCode.NotFound, productionIssuance.StatusCode);
            var productionValidation = await ValidateAsync(
                productionClient,
                firstIssue.Token,
                DevelopmentLicenseConstants.TenantCode,
                [DevelopmentLicenseConstants.PlatformFeature],
                HttpStatusCode.Forbidden);
            Assert.False(productionValidation.IsValid);
            Assert.Equal("licensing.development_license_forbidden", productionValidation.ErrorCode);

            var revokeResponse = await developmentClient.PostAsync(
                $"/api/licenses/{firstIssue.Descriptor.LicenseId:D}/revoke",
                content: null);
            var revocation = await ReadAsync<LicenseRevocationResponse>(revokeResponse, HttpStatusCode.OK);
            Assert.Equal(LicenseStatus.Revoked, revocation.Status);

            var revoked = await ValidateAsync(
                developmentClient,
                firstIssue.Token,
                DevelopmentLicenseConstants.TenantCode,
                [DevelopmentLicenseConstants.WorkerFeature],
                HttpStatusCode.Forbidden);
            Assert.False(revoked.IsValid);
            Assert.Equal("licensing.revoked", revoked.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>검증 API가 지정된 상태 코드와 공개 오류 계약을 함께 반환하는지 확인합니다.</summary>
    private static async Task<LicenseValidationResult> ValidateAsync(
        HttpClient client,
        string token,
        string tenant,
        IReadOnlyCollection<string> features,
        HttpStatusCode expectedStatus)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/licenses/validate",
            new
            {
                Token = token,
                SiteOrTenant = tenant,
                Product = DevelopmentLicenseConstants.Product,
                RequiredFeatures = features
            });

        var result = await ReadAsync<LicenseValidationResult>(response, expectedStatus);
        return result;
    }

    /// <summary>지정된 상태 코드의 JSON 응답을 공개 계약 형식으로 읽습니다.</summary>
    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, HttpStatusCode expectedStatus)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        var value = await response.Content.ReadFromJsonAsync<T>();
        return Assert.IsType<T>(value);
    }

    /// <summary>서명 바이트 하나를 바꾸어 위조된 토큰을 만듭니다.</summary>
    private static string TamperSignature(string token)
    {
        var parts = token.Split('.');
        var signature = FileEs256KeyMaterial.FromBase64Url(parts[2]);
        signature[0] ^= 1;
        return $"{parts[0]}.{parts[1]}.{FileEs256KeyMaterial.Base64Url(signature)}";
    }

    /// <summary>서명과 Claim 본문은 유지하고 등록되지 않은 KeyId를 헤더에 넣습니다.</summary>
    private static string ReplaceKeyId(string token, string keyId)
    {
        var parts = token.Split('.');
        var header = FileEs256KeyMaterial.Base64Url(
            Encoding.UTF8.GetBytes($"{{\"alg\":\"ES256\",\"kid\":\"{keyId}\",\"typ\":\"JWT\"}}"));
        return $"{header}.{parts[1]}.{parts[2]}";
    }

    /// <summary>HTTP 검증에 사용할 공개 라이선스 정보를 생성합니다.</summary>
    private static LicenseDescriptor CreateDescriptor(
        ILicenseTokenCodec codec,
        DateTimeOffset startsAtUtc,
        DateTimeOffset expiresAtUtc) =>
        new(
            Guid.NewGuid(),
            DevelopmentLicenseConstants.TenantCode,
            DevelopmentLicenseConstants.Product,
            DevelopmentLicenseConstants.Features,
            startsAtUtc,
            startsAtUtc,
            expiresAtUtc,
            codec.KeyId,
            LicenseStatus.Active);

    /// <summary>임시 키와 파일 저장소를 사용하는 LicenseAuthority 테스트 Host를 생성합니다.</summary>
    private sealed class LicenseAuthorityFactory(string root, string environment) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment(environment);

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureHostConfiguration(configuration => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["LicenseAuthority:EnableDevelopmentIssuance"] =
                        string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase).ToString(),
                    ["LicenseAuthority:KeyId"] = "http-test-key",
                    ["LicenseAuthority:PrivateKeyPath"] = Path.Combine(root, "authority.pem"),
                    ["LicenseAuthority:DevelopmentLicensePath"] = Path.Combine(root, "development-license.json"),
                    ["LicenseAuthority:StatusPath"] = Path.Combine(root, "license-status.json")
                }));
            return base.CreateHost(builder);
        }
    }
}
