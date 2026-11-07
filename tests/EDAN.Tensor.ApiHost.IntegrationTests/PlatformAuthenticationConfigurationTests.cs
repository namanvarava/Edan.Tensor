using EDAN.Tensor.ApiHost.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace EDAN.Tensor.ApiHost.IntegrationTests;

/// <summary>인증 설정 누락 시 보호 API가 닫힌 상태를 유지하는지 검증합니다.</summary>
public sealed class PlatformAuthenticationConfigurationTests
{
    /// <summary>Issuer와 Audience가 없으면 JWT 발급자와 대상 검증을 끄지 않습니다.</summary>
    [Fact]
    public void MissingIssuerOrAudienceKeepsJwtValidationClosed()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:SigningKey"] = "integration-test-signing-key-at-least-32-bytes-long"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddPlatformSecurity(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.True(options.TokenValidationParameters.ValidateIssuer);
        Assert.True(options.TokenValidationParameters.ValidateAudience);
        Assert.Null(options.TokenValidationParameters.ValidIssuer);
        Assert.Null(options.TokenValidationParameters.ValidAudience);
        Assert.True(options.TokenValidationParameters.ValidateIssuerSigningKey);
    }

    /// <summary>로컬 테스트 IdP의 OIDC 메타데이터 주소를 사용하되 운영 HTTPS 요구 기본값을 보존합니다.</summary>
    [Fact]
    public void LocalMetadataAddressEnablesJwksDiscoveryWithoutDisablingValidation()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Issuer"] = "http://127.0.0.1:5001",
                ["Authentication:Audience"] = "edan-tensor-api",
                ["Authentication:MetadataAddress"] = "http://127.0.0.1:5001/.well-known/openid-configuration",
                ["Authentication:RequireHttpsMetadata"] = "false"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddPlatformSecurity(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.Equal("http://127.0.0.1:5001/.well-known/openid-configuration", options.MetadataAddress);
        Assert.False(options.RequireHttpsMetadata);
        Assert.True(options.RefreshOnIssuerKeyNotFound);
        Assert.True(options.TokenValidationParameters.ValidateIssuer);
        Assert.True(options.TokenValidationParameters.ValidateAudience);
        Assert.True(options.TokenValidationParameters.ValidateIssuerSigningKey);
    }

    /// <summary>개발 Bypass 모드는 외부 JWT 대신 전용 인증 Handler를 선택합니다.</summary>
    [Fact]
    public async Task DevelopmentBypassSelectsDedicatedAuthenticationScheme()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Mode"] = "Bypass",
                ["Authentication:BypassSubject"] = "local-development-subject",
                ["Authentication:BypassRole"] = "platform_admin"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddPlatformSecurity(configuration, new TestHostEnvironment(Environments.Development));

        using var provider = services.BuildServiceProvider();
        var authentication = provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
        var scheme = await provider.GetRequiredService<IAuthenticationSchemeProvider>()
            .GetSchemeAsync("DevelopmentBypass");

        Assert.Equal("DevelopmentBypass", authentication.DefaultAuthenticateScheme);
        Assert.Equal("DevelopmentBypass", authentication.DefaultChallengeScheme);
        Assert.Equal("EDAN.Tensor.ApiHost.Security.DevelopmentBypassAuthenticationHandler", scheme?.HandlerType.FullName);
    }

    /// <summary>운영 환경은 개발 Bypass 인증을 등록하지 않습니다.</summary>
    [Fact]
    public void BypassIsRejectedOutsideDevelopment()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Mode"] = "Bypass"
            })
            .Build();
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddPlatformSecurity(configuration, new TestHostEnvironment(Environments.Production)));

        Assert.Contains("Development", exception.Message, StringComparison.Ordinal);
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = typeof(PlatformAuthenticationConfigurationTests).Assembly.GetName().Name!;
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
