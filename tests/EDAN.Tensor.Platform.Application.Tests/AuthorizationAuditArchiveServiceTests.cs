using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Application.UseCases;
using EDAN.Tensor.Platform.Domain.Model;
using Xunit;

namespace EDAN.Tensor.Platform.Application.Tests;

/// <summary>감사 아카이브 생성과 Tenant 경계 및 체크섬 검증을 확인합니다.</summary>
public sealed class AuthorizationAuditArchiveServiceTests
{
    [Fact]
    public async Task CreatesAndVerifiesArchiveWithoutChangingSourceRecords()
    {
        var store = new MemoryArchiveStore();
        var service = new AuthorizationAuditArchiveService(new EmptyAuditRepository(), store);
        var tenantId = new TenantId(Guid.NewGuid());

        var archive = await service.CreateAsync(tenantId,
            DateTimeOffset.Parse("2026-11-01T00:00:00Z"),
            DateTimeOffset.Parse("2026-11-02T00:00:00Z"));
        var verification = await service.VerifyAsync(tenantId, archive.ArchiveId, archive.Sha256);

        Assert.Equal(0, archive.RecordCount);
        Assert.Equal(64, archive.Sha256.Length);
        Assert.True(verification.IsValid);
        Assert.Equal("archive.verified", verification.ReasonCode);
    }

    [Fact]
    public async Task RejectsArchiveVerificationAcrossTenantBoundary()
    {
        var store = new MemoryArchiveStore();
        var service = new AuthorizationAuditArchiveService(new EmptyAuditRepository(), store);
        var tenantId = new TenantId(Guid.NewGuid());
        var archive = await service.CreateAsync(tenantId,
            DateTimeOffset.Parse("2026-11-01T00:00:00Z"),
            DateTimeOffset.Parse("2026-11-02T00:00:00Z"));

        var result = await service.VerifyAsync(new TenantId(Guid.NewGuid()), archive.ArchiveId);

        Assert.False(result.IsValid);
        Assert.Equal("archive.tenant_mismatch", result.ReasonCode);
    }

    [Fact]
    public async Task RejectsExternalChecksumMismatch()
    {
        var service = new AuthorizationAuditArchiveService(new EmptyAuditRepository(), new MemoryArchiveStore());
        var tenantId = new TenantId(Guid.NewGuid());
        var archive = await service.CreateAsync(tenantId,
            DateTimeOffset.Parse("2026-11-01T00:00:00Z"),
            DateTimeOffset.Parse("2026-11-02T00:00:00Z"));

        var result = await service.VerifyAsync(tenantId, archive.ArchiveId, new string('0', 64));

        Assert.False(result.IsValid);
        Assert.Equal("archive.external_checksum_mismatch", result.ReasonCode);
    }

    [Fact]
    public async Task RejectsArchiveWindowLongerThanThirtyOneDays()
    {
        var service = new AuthorizationAuditArchiveService(new EmptyAuditRepository(), new MemoryArchiveStore());

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new TenantId(Guid.NewGuid()),
            DateTimeOffset.Parse("2026-10-01T00:00:00Z"),
            DateTimeOffset.Parse("2026-11-02T00:00:00Z")));
    }

    private sealed class EmptyAuditRepository : IAuthorizationAuditRepository
    {
        public Task<AuthorizationAuditPage> GetPageAsync(TenantId tenantId, string? subjectId, string? action,
            bool? isAllowed, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, int page, int pageSize,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AuthorizationAuditPage([], 0, page, pageSize));

        public Task<IReadOnlyList<AuthorizationAuditEntry>> GetForIntegrityAsync(TenantId tenantId,
            DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AuthorizationAuditEntry>>([]);

        public void Add(AuthorizationAuditEntry entry) => throw new NotSupportedException();
    }

    private sealed class MemoryArchiveStore : IAuthorizationAuditArchiveStore
    {
        private readonly Dictionary<Guid, byte[]> _items = [];

        public Task WriteAsync(Guid archiveId, ReadOnlyMemory<byte> content,
            CancellationToken cancellationToken = default)
        {
            if (!_items.TryAdd(archiveId, content.ToArray()))
                throw new InvalidOperationException("아카이브 식별자가 중복되었습니다.");
            return Task.CompletedTask;
        }

        public Task<byte[]?> ReadAsync(Guid archiveId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.TryGetValue(archiveId, out var value) ? value : null);
    }
}
