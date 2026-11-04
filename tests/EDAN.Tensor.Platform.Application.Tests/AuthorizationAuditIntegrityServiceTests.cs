using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Application.UseCases;
using EDAN.Tensor.Platform.Domain.Model;
using Xunit;

namespace EDAN.Tensor.Platform.Application.Tests;

/// <summary>감사 기록 무결성 체크섬 생성과 검증 규칙을 확인합니다.</summary>
public sealed class AuthorizationAuditIntegrityServiceTests
{
    [Fact]
    public async Task CalculatesStableChecksumForTenantAuditWindow()
    {
        var service = CreateService();
        var tenantId = new TenantId(Guid.NewGuid());
        var fromUtc = DateTimeOffset.Parse("2026-11-01T00:00:00Z");
        var toUtc = DateTimeOffset.Parse("2026-11-02T00:00:00Z");

        var first = await service.CalculateAsync(tenantId, fromUtc, toUtc);
        var second = await service.CalculateAsync(tenantId, fromUtc, toUtc);

        Assert.Equal(first.Sha256, second.Sha256);
        Assert.Equal(64, first.Sha256.Length);
        Assert.Equal(0, first.RecordCount);
        Assert.Equal(tenantId.Value, first.TenantId);
    }

    [Fact]
    public async Task VerifiesMatchingAndRejectsDifferentChecksum()
    {
        var service = CreateService();
        var tenantId = new TenantId(Guid.NewGuid());
        var fromUtc = DateTimeOffset.Parse("2026-11-01T00:00:00Z");
        var toUtc = DateTimeOffset.Parse("2026-11-02T00:00:00Z");
        var baseline = await service.CalculateAsync(tenantId, fromUtc, toUtc);

        var valid = await service.VerifyAsync(tenantId, fromUtc, toUtc, baseline.Sha256);
        var invalid = await service.VerifyAsync(tenantId, fromUtc, toUtc, new string('0', 64));

        Assert.True(valid.IsValid);
        Assert.False(invalid.IsValid);
    }

    [Fact]
    public async Task RejectsIntegrityWindowLongerThanThirtyOneDays()
    {
        var service = CreateService();
        var tenantId = new TenantId(Guid.NewGuid());

        await Assert.ThrowsAsync<ArgumentException>(() => service.CalculateAsync(tenantId,
            DateTimeOffset.Parse("2026-10-01T00:00:00Z"),
            DateTimeOffset.Parse("2026-11-02T00:00:00Z")));
    }

    private static AuthorizationAuditIntegrityService CreateService() =>
        new(new AuthorizationAuditQueryService(new EmptyAuditRepository()));

    private sealed class EmptyAuditRepository : IAuthorizationAuditRepository
    {
        public Task<AuthorizationAuditPage> GetPageAsync(TenantId tenantId, string? subjectId, string? action,
            bool? isAllowed, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, int page, int pageSize,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AuthorizationAuditPage([], 0, page, pageSize));

        public void Add(AuthorizationAuditEntry entry) => throw new NotSupportedException();
    }
}
