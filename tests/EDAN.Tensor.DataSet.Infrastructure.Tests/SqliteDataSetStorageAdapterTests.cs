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
            await adapter.WriteAsync(allocation, reference, source);
            await using var result = await adapter.OpenReadAsync(allocation, reference);
            Assert.NotNull(result);
            using var reader = new StreamReader(result!);
            Assert.Equal("tensor-payload", await reader.ReadToEndAsync());

            var usage = await adapter.GetUsageAsync(allocation);
            Assert.Equal(14, usage.TenantBytes);
            Assert.Equal(1, usage.TenantDataSetCount);
            Assert.Equal(14, usage.OrganizationBytes);
            Assert.Equal(1, usage.OrganizationDataSetCount);
            Assert.Equal(14, usage.ProjectBytes);
            Assert.Equal(1, usage.ProjectDataSetCount);
            Assert.Equal(14, usage.DataSetBytes);

            await adapter.ReleaseAsync(allocation, reference);
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
            await Assert.ThrowsAsync<KeyNotFoundException>(() => adapter.WriteAsync(otherTenant, reference, source));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static DataSetStorageAllocation Allocation() =>
        new(new DataSetId(Guid.NewGuid()), new DataSetScope(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
}
