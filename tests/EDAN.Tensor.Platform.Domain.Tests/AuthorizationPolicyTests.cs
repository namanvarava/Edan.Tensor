using EDAN.Tensor.Platform.Domain.Model;
using Xunit;

namespace EDAN.Tensor.Platform.Domain.Tests;

/// <summary>접근 정책의 범위, 효과 및 입력 검증 규칙을 확인합니다.</summary>
public sealed class AuthorizationPolicyTests
{
    [Fact]
    public void PolicyNormalizesActionAndRetainsScope()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var organizationId = new OrganizationId(Guid.NewGuid());
        var policy = AuthorizationPolicy.Create(new AuthorizationPolicyId(Guid.NewGuid()), tenantId,
            organizationId, null, "subject-1", " Resource:Create ", "DataSet",
            AuthorizationPolicyEffect.Deny, 100, DateTimeOffset.UtcNow);

        Assert.Equal("resource:create", policy.Action);
        Assert.Equal(tenantId, policy.TenantId);
        Assert.Equal(organizationId, policy.OrganizationId);
        Assert.True(policy.IsEnabled);
    }

    [Fact]
    public void ProjectPolicyRequiresOrganizationScope()
    {
        Assert.Throws<ArgumentException>(() => AuthorizationPolicy.Create(
            new AuthorizationPolicyId(Guid.NewGuid()), new TenantId(Guid.NewGuid()),
            null, new ProjectId(Guid.NewGuid()), "subject-1", "resource:create", "*",
            AuthorizationPolicyEffect.Allow, 0, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void PolicyRejectsPriorityOutsideSupportedRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AuthorizationPolicy.Create(
            new AuthorizationPolicyId(Guid.NewGuid()), new TenantId(Guid.NewGuid()),
            null, null, "subject-1", "resource:create", "*",
            AuthorizationPolicyEffect.Allow, 10001, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void PolicyCanBeDisabledWithoutChangingItsDefinition()
    {
        var policy = AuthorizationPolicy.Create(new AuthorizationPolicyId(Guid.NewGuid()),
            new TenantId(Guid.NewGuid()), null, null, "subject-1", "resource:create", "*",
            AuthorizationPolicyEffect.Allow, 10, DateTimeOffset.UtcNow);

        policy.SetEnabled(false);

        Assert.False(policy.IsEnabled);
        Assert.Equal(AuthorizationPolicyEffect.Allow, policy.Effect);
    }
}
