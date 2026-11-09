using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Exceptions;
using Xunit;

namespace EDAN.Tensor.Platform.Domain.Tests;

/// <summary>저장 용량 정책의 입력·버전·활성 상태 업무 규칙을 검증합니다.</summary>
public sealed class StorageQuotaPolicyTests
{
    [Fact]
    public void UpdateWithStaleVersionReturnsStableConflictCode()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var policy = StorageQuotaPolicy.Create(Guid.NewGuid(), tenantId, StorageQuotaPolicyScope.Tenant,
            tenantId.Value, 1024, null, null, null, 80, "Community", "subject-1", DateTimeOffset.UtcNow);

        var exception = Assert.Throws<BusinessRuleViolationException>(() => policy.Update(
            2048, null, null, null, 80, "Community", "subject-1", 0, DateTimeOffset.UtcNow));

        Assert.Equal("quota_policy.version_conflict", exception.Code);
        Assert.Equal(1, policy.Version);
        Assert.Equal(1024, policy.TenantLimitBytes);
    }

    [Fact]
    public void SetEnabledWithStaleVersionReturnsStableConflictCode()
    {
        var tenantId = new TenantId(Guid.NewGuid());
        var policy = StorageQuotaPolicy.Create(Guid.NewGuid(), tenantId, StorageQuotaPolicyScope.Tenant,
            tenantId.Value, 1024, null, null, null, 80, "Community", "subject-1", DateTimeOffset.UtcNow);

        var exception = Assert.Throws<BusinessRuleViolationException>(() => policy.SetEnabled(
            false, "subject-1", 0, DateTimeOffset.UtcNow));

        Assert.Equal("quota_policy.version_conflict", exception.Code);
        Assert.True(policy.IsEnabled);
        Assert.Equal(1, policy.Version);
    }
}
