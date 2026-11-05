using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Application.UseCases;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Time;
using Xunit;

namespace EDAN.Tensor.Platform.Application.Tests;

/// <summary>감사 보존 정책의 기본값, 변경 이력 및 법적 보존 검증을 확인합니다.</summary>
public sealed class AuthorizationAuditRetentionServiceTests
{
    [Fact]
    public async Task ReturnsDefaultPolicyWhenTenantHasNoSavedPolicy()
    {
        var service = CreateService(new RetentionRepository());
        var result = await service.GetAsync(new TenantId(Guid.NewGuid()));

        Assert.Equal(365, result.RetentionDays);
        Assert.False(result.IsLegalHold);
        Assert.Equal(0, result.Version);
    }

    [Fact]
    public async Task CreatesPolicyAndRecordsChange()
    {
        var repository = new RetentionRepository();
        var service = CreateService(repository);
        var tenantId = new TenantId(Guid.NewGuid());

        var result = await service.UpdateAsync(tenantId, 730, false, null, "admin-subject");

        Assert.Equal(730, result.RetentionDays);
        Assert.Equal(1, result.Version);
        Assert.Single(repository.Changes);
        Assert.Equal("Created", repository.Changes[0].ChangeType);
    }

    [Fact]
    public async Task RequiresReasonForLegalHold()
    {
        var service = CreateService(new RetentionRepository());

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateAsync(
            new TenantId(Guid.NewGuid()), 365, true, null, "admin-subject"));
    }

    [Theory]
    [InlineData(29)]
    [InlineData(3651)]
    public async Task RejectsRetentionOutsideSupportedRange(int days)
    {
        var service = CreateService(new RetentionRepository());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.UpdateAsync(
            new TenantId(Guid.NewGuid()), days, false, null, "admin-subject"));
    }

    private static AuthorizationAuditRetentionService CreateService(RetentionRepository repository) =>
        new(repository, new FakeUnitOfWork(), new FakeClock());

    private sealed class RetentionRepository : IAuthorizationAuditRetentionRepository
    {
        public AuthorizationAuditRetentionPolicy? Policy { get; private set; }
        public List<AuthorizationAuditRetentionChange> Changes { get; } = [];

        public Task<AuthorizationAuditRetentionPolicy?> GetByTenantAsync(TenantId tenantId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Policy?.TenantId == tenantId ? Policy : null);

        public void Add(AuthorizationAuditRetentionPolicy policy) => Policy = policy;
        public void AddChange(AuthorizationAuditRetentionChange change) => Changes.Add(change);
    }

    private sealed class FakeUnitOfWork : IPlatformUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class FakeClock : ISystemClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.Parse("2026-11-05T00:00:00Z");
    }
}
