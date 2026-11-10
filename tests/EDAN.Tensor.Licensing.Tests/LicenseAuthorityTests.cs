using EDAN_Tensor_Licensing_Application.Abstractions;
using EDAN_Tensor_Licensing_Application.Services;
using EDAN_Tensor_Licensing_Contracts;
using EDAN_Tensor_Licensing_Infrastructure.Cryptography;
using EDAN_Tensor_Licensing_Infrastructure.Persistence;
using System.Text;
using Xunit;

namespace EDAN_Tensor_Licensing_Tests;

/// <summary>개발 라이선스 발급·서명·검증·폐기 계약을 검증합니다.</summary>
public sealed class LicenseAuthorityTests
{
    /// <summary>개발 라이선스가 승인된 Tenant·제품·기능과 5년 유효 기간을 사용합니다.</summary>
    [Fact]
    public async Task DevelopmentLicense_UsesApprovedFiveYearContract()
    {
        using var fixture = CreateFixture();
        var now = new DateTimeOffset(2030, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var service = new DevelopmentLicenseService(fixture.Store, fixture.Codec);

        var result = await service.IssueOrGetAsync(now);

        Assert.True(result.IsSuccess);
        Assert.Equal(DevelopmentLicenseConstants.TenantCode, result.Value.Descriptor.SiteOrTenant);
        Assert.Equal(DevelopmentLicenseConstants.Product, result.Value.Descriptor.Product);
        Assert.Equal(DevelopmentLicenseConstants.Features, result.Value.Descriptor.Features);
        Assert.Equal(now, result.Value.Descriptor.StartsAtUtc);
        Assert.Equal(now.AddYears(5), result.Value.Descriptor.ExpiresAtUtc);
        Assert.Equal(LicenseStatus.Active, result.Value.Descriptor.Status);
    }

    /// <summary>동일한 개발 라이선스 발급 요청은 저장된 공개 토큰을 재사용합니다.</summary>
    [Fact]
    public async Task DevelopmentLicense_ReusesSingleStoredLicense()
    {
        using var fixture = CreateFixture();
        var service = new DevelopmentLicenseService(fixture.Store, fixture.Codec);
        var now = new DateTimeOffset(2030, 1, 2, 3, 4, 5, TimeSpan.Zero);

        var first = await service.IssueOrGetAsync(now);
        var second = await service.IssueOrGetAsync(now.AddDays(1));

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(first.Value.Token, second.Value.Token);
        Assert.Equal(first.Value.Descriptor.LicenseId, second.Value.Descriptor.LicenseId);
    }

    /// <summary>정상 토큰은 Tenant·제품·기능이 일치할 때 검증됩니다.</summary>
    [Fact]
    public async Task Validation_AcceptsMatchingTenantProductAndFeature()
    {
        using var fixture = CreateFixture();
        var service = new DevelopmentLicenseService(fixture.Store, fixture.Codec);
        var validation = new LicenseValidationService(fixture.Codec, fixture.StatusStore);
        var now = new DateTimeOffset(2030, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var issued = await service.IssueOrGetAsync(now);

        var result = await validation.ValidateAsync(
            issued.Value.Token,
            new LicenseValidationRequest(
                DevelopmentLicenseConstants.TenantCode,
                DevelopmentLicenseConstants.Product,
                [DevelopmentLicenseConstants.DataSetFeature]),
            now.AddMinutes(1));

        Assert.True(result.IsValid);
        Assert.Equal(issued.Value.Descriptor.LicenseId, result.Descriptor!.LicenseId);
    }

    /// <summary>다른 Tenant와 기능은 각각 안정적인 오류 코드로 거부됩니다.</summary>
    [Fact]
    public async Task Validation_RejectsTenantAndFeatureMismatch()
    {
        using var fixture = CreateFixture();
        var service = new DevelopmentLicenseService(fixture.Store, fixture.Codec);
        var validation = new LicenseValidationService(fixture.Codec, fixture.StatusStore);
        var now = new DateTimeOffset(2030, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var issued = await service.IssueOrGetAsync(now);

        var tenantResult = await validation.ValidateAsync(
            issued.Value.Token,
            new LicenseValidationRequest("OTHER-TENANT", DevelopmentLicenseConstants.Product, [DevelopmentLicenseConstants.PlatformFeature]),
            now);
        var featureResult = await validation.ValidateAsync(
            issued.Value.Token,
            new LicenseValidationRequest(DevelopmentLicenseConstants.TenantCode, DevelopmentLicenseConstants.Product, ["Commercial"]),
            now);

        Assert.False(tenantResult.IsValid);
        Assert.Equal("licensing.tenant_mismatch", tenantResult.ErrorCode);
        Assert.False(featureResult.IsValid);
        Assert.Equal("licensing.feature_not_entitled", featureResult.ErrorCode);
    }

    /// <summary>서명 토큰은 위조되거나 만료되면 거부됩니다.</summary>
    [Fact]
    public async Task Validation_RejectsTamperedAndExpiredTokens()
    {
        using var fixture = CreateFixture();
        var service = new DevelopmentLicenseService(fixture.Store, fixture.Codec);
        var validation = new LicenseValidationService(fixture.Codec, fixture.StatusStore);
        var now = new DateTimeOffset(2030, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var issued = await service.IssueOrGetAsync(now);

        var tokenParts = issued.Value.Token.Split('.');
        var tamperedSignature = FileEs256KeyMaterial.FromBase64Url(tokenParts[2]);
        tamperedSignature[0] ^= 1;
        var tampered = $"{tokenParts[0]}.{tokenParts[1]}.{FileEs256KeyMaterial.Base64Url(tamperedSignature)}";
        var tamperedResult = await validation.ValidateAsync(
            tampered,
            new LicenseValidationRequest(DevelopmentLicenseConstants.TenantCode, DevelopmentLicenseConstants.Product, [DevelopmentLicenseConstants.PlatformFeature]),
            now);
        var expiredResult = await validation.ValidateAsync(
            issued.Value.Token,
            new LicenseValidationRequest(DevelopmentLicenseConstants.TenantCode, DevelopmentLicenseConstants.Product, [DevelopmentLicenseConstants.PlatformFeature]),
            now.AddYears(5));

        Assert.False(tamperedResult.IsValid);
        Assert.Equal("licensing.token.signature_invalid", tamperedResult.ErrorCode);
        Assert.False(expiredResult.IsValid);
        Assert.Equal("licensing.expired", expiredResult.ErrorCode);
    }

    /// <summary>사용 시작 시각이 미래인 토큰은 사용 시작 전 오류로 거부됩니다.</summary>
    [Fact]
    public async Task Validation_RejectsNotYetValidToken()
    {
        using var fixture = CreateFixture();
        var validation = new LicenseValidationService(fixture.Codec, fixture.StatusStore);
        var now = new DateTimeOffset(2030, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var descriptor = new LicenseDescriptor(
            Guid.NewGuid(),
            DevelopmentLicenseConstants.TenantCode,
            DevelopmentLicenseConstants.Product,
            DevelopmentLicenseConstants.Features,
            now,
            now.AddMinutes(5),
            now.AddYears(5),
            fixture.Codec.KeyId,
            LicenseStatus.Active);

        var result = await validation.ValidateAsync(
            fixture.Codec.CreateToken(descriptor),
            new LicenseValidationRequest(DevelopmentLicenseConstants.TenantCode, DevelopmentLicenseConstants.Product, [DevelopmentLicenseConstants.PlatformFeature]),
            now);

        Assert.False(result.IsValid);
        Assert.Equal("licensing.not_yet_valid", result.ErrorCode);
    }

    /// <summary>공개키에 등록되지 않은 KeyId는 서명 검증 전에 거부됩니다.</summary>
    [Fact]
    public async Task Validation_RejectsUnknownKeyId()
    {
        using var fixture = CreateFixture();
        var service = new DevelopmentLicenseService(fixture.Store, fixture.Codec);
        var validation = new LicenseValidationService(fixture.Codec, fixture.StatusStore);
        var now = new DateTimeOffset(2030, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var issued = await service.IssueOrGetAsync(now);
        var tokenParts = issued.Value.Token.Split('.');
        var unknownHeader = FileEs256KeyMaterial.Base64Url(Encoding.UTF8.GetBytes(
            $"{{\"alg\":\"ES256\",\"kid\":\"unknown-key\",\"typ\":\"JWT\"}}"));
        var tokenWithUnknownKey = $"{unknownHeader}.{tokenParts[1]}.{tokenParts[2]}";

        var result = await validation.ValidateAsync(
            tokenWithUnknownKey,
            new LicenseValidationRequest(DevelopmentLicenseConstants.TenantCode, DevelopmentLicenseConstants.Product, [DevelopmentLicenseConstants.PlatformFeature]),
            now);

        Assert.False(result.IsValid);
        Assert.Equal("licensing.token.key_invalid", result.ErrorCode);
    }

    /// <summary>개발 Tenant 라이선스는 운영 환경 정책에서 거부됩니다.</summary>
    [Fact]
    public void EnvironmentPolicy_RejectsDevelopmentLicenseInProduction()
    {
        var descriptor = new LicenseDescriptor(
            Guid.NewGuid(),
            DevelopmentLicenseConstants.TenantCode,
            DevelopmentLicenseConstants.Product,
            DevelopmentLicenseConstants.Features,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddYears(5),
            "test-key",
            LicenseStatus.Active);

        Assert.False(LicenseEnvironmentPolicy.IsAllowed(false, descriptor));
        Assert.True(LicenseEnvironmentPolicy.IsAllowed(true, descriptor));
    }

    /// <summary>폐기 상태는 서명이 정상이어도 사용을 거부합니다.</summary>
    [Fact]
    public async Task Validation_RejectsRevokedLicense()
    {
        using var fixture = CreateFixture();
        var service = new DevelopmentLicenseService(fixture.Store, fixture.Codec);
        var validation = new LicenseValidationService(fixture.Codec, fixture.StatusStore);
        var now = new DateTimeOffset(2030, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var issued = await service.IssueOrGetAsync(now);
        await fixture.StatusStore.RevokeAsync(issued.Value.Descriptor.LicenseId);

        var result = await validation.ValidateAsync(
            issued.Value.Token,
            new LicenseValidationRequest(DevelopmentLicenseConstants.TenantCode, DevelopmentLicenseConstants.Product, [DevelopmentLicenseConstants.WorkerFeature]),
            now);

        Assert.False(result.IsValid);
        Assert.Equal("licensing.revoked", result.ErrorCode);
    }

    private static Fixture CreateFixture()
    {
        var directory = Path.Combine(Path.GetTempPath(), "edan-license-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var key = new FileEs256KeyMaterial(Path.Combine(directory, "authority.pem"));
        var codec = new Es256LicenseTokenCodec(key);
        return new Fixture(
            key,
            codec,
            new FileDevelopmentLicenseStore(Path.Combine(directory, "license.json")),
            new FileLicenseStatusStore(Path.Combine(directory, "status.json")),
            directory);
    }

    private sealed record Fixture(
        FileEs256KeyMaterial Key,
        ILicenseTokenCodec Codec,
        IDevelopmentLicenseStore Store,
        FileLicenseStatusStore StatusStore,
        string DirectoryPath) : IDisposable
    {
        public void Dispose()
        {
            Key.Dispose();
            if (Directory.Exists(DirectoryPath))
                Directory.Delete(DirectoryPath, recursive: true);
        }
    }
}

