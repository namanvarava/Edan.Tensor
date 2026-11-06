using EDAN.Tensor.ApiHost.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
}
