using EDAN.Tensor.DataSet.Domain.Model;
using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Infrastructure.Storage;
using Xunit;

namespace EDAN.Tensor.DataSet.Infrastructure.Tests;

public sealed class SqliteDataSetStorageAdapterTests
{
    [Fact]
    public async Task AllocatedContentCanBeWrittenReadAndReleased()
    {
        var path = Path.Combine(Path.GetTempPath(), "edan-content-" + Guid.NewGuid().ToString("N") + ".db");
        try
        {
            var adapter = new SqliteDataSetStorageAdapter("Data Source=" + path);
            var allocation = Allocation();
            var reference = await adapter.AllocateAsync(allocation);
            await using var source = new MemoryStream("tensor-payload"u8.ToArray());
            await adapter.WriteAsync(allocation, reference, source, Unlimited());
            await using var result = await adapter.OpenReadAsync(allocation, reference);
            Assert.NotNull(result);
            using var reader = new StreamReader(result!);
            Assert.Equal("tensor-payload", await reader.ReadToEndAsync());

            var secondAllocation = new DataSetStorageAllocation(new DataSetId(Guid.NewGuid()), allocation.Scope);
            var secondReference = await adapter.AllocateAsync(secondAllocation);
            await using var secondContent = new MemoryStream("other"u8.ToArray());
            await adapter.WriteAsync(secondAllocation, secondReference, secondContent, Unlimited());

            var usage = await adapter.GetUsageAsync(allocation);
            Assert.Equal(19, usage.TenantBytes);
            Assert.Equal(2, usage.TenantDataSetCount);
            Assert.Equal(19, usage.OrganizationBytes);
            Assert.Equal(2, usage.OrganizationDataSetCount);
            Assert.Equal(19, usage.ProjectBytes);
            Assert.Equal(2, usage.ProjectDataSetCount);
            Assert.Equal(14, usage.DataSetBytes);

            await adapter.ReleaseAsync(allocation, reference);
            var remainingUsage = await adapter.GetUsageAsync(allocation);
            Assert.Equal(5, remainingUsage.TenantBytes);
            Assert.Equal(1, remainingUsage.TenantDataSetCount);
            Assert.Equal(5, remainingUsage.ProjectBytes);
            Assert.Equal(0, remainingUsage.DataSetBytes);

            await adapter.ReleaseAsync(secondAllocation, secondReference);
            var emptyUsage = await adapter.GetUsageAsync(allocation);
            Assert.Equal(0, emptyUsage.TenantBytes);
            Assert.Equal(0, emptyUsage.TenantDataSetCount);
            Assert.Equal(0, emptyUsage.OrganizationBytes);
            Assert.Equal(0, emptyUsage.OrganizationDataSetCount);
            Assert.Equal(0, emptyUsage.ProjectBytes);
            Assert.Equal(0, emptyUsage.ProjectDataSetCount);
            Assert.Equal(0, emptyUsage.DataSetBytes);
            Assert.Null(await adapter.OpenReadAsync(allocation, reference));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task ContentCannotBeReadOrWrittenAcrossTenantScope()
    {
        var path = Path.Combine(Path.GetTempPath(), "edan-content-" + Guid.NewGuid().ToString("N") + ".db");
        try
        {
            var adapter = new SqliteDataSetStorageAdapter("Data Source=" + path);
            var allocation = Allocation();
            var reference = await adapter.AllocateAsync(allocation);
            var otherTenant = new DataSetStorageAllocation(allocation.DataSetId,
                new DataSetScope(Guid.NewGuid(), allocation.Scope.OrganizationId, allocation.Scope.ProjectId));
            Assert.Null(await adapter.OpenReadAsync(otherTenant, reference));
            await using var source = new MemoryStream("blocked"u8.ToArray());
            await Assert.ThrowsAsync<KeyNotFoundException>(() => adapter.WriteAsync(otherTenant, reference, source, Unlimited()));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task QuotaIsEnforcedAtomicallyAndReplacementDoesNotDoubleCountExistingContent()
    {
        var path = Path.Combine(Path.GetTempPath(), "edan-content-quota-" + Guid.NewGuid().ToString("N") + ".db");
        try
        {
            var adapter = new SqliteDataSetStorageAdapter("Data Source=" + path);
            var allocation = Allocation();
            var reference = await adapter.AllocateAsync(allocation);
            var policy = new DataSetStorageQuotaPolicy(10, 10, 10, 10);
            await using (var first = new MemoryStream("12345678"u8.ToArray()))
                await adapter.WriteAsync(allocation, reference, first, policy);
            await using (var replacement = new MemoryStream("1234567890"u8.ToArray()))
                await Assert.ThrowsAsync<DataSetStorageQuotaExceededException>(() => adapter.WriteAsync(allocation, reference, replacement, policy));
            await using var existing = await adapter.OpenReadAsync(allocation, reference);
            Assert.NotNull(existing);
            Assert.Equal(8, existing!.Length);
            var secondAllocation = new DataSetStorageAllocation(new DataSetId(Guid.NewGuid()), allocation.Scope);
            var secondReference = await adapter.AllocateAsync(secondAllocation);
            await using var second = new MemoryStream("123"u8.ToArray());
            await Assert.ThrowsAsync<DataSetStorageQuotaExceededException>(() => adapter.WriteAsync(secondAllocation, secondReference, second, policy));
            var usage = await adapter.GetUsageAsync(allocation);
            Assert.Equal(8, usage.TenantBytes);
            Assert.Equal(8, usage.ProjectBytes);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    private static DataSetStorageQuotaPolicy Unlimited() => new(null, null, null, null);

    private static DataSetStorageAllocation Allocation() =>
        new(new DataSetId(Guid.NewGuid()), new DataSetScope(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
}
