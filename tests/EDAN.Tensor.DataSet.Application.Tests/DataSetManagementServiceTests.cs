using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Application.UseCases;
using EDAN.Tensor.DataSet.Domain.Model;
using Xunit;
using DataSetAggregate = EDAN.Tensor.DataSet.Domain.Model.DataSet;

namespace EDAN.Tensor.DataSet.Application.Tests;

/// <summary>DataSet 유스케이스의 소유권 검증과 저장소 보상 동작을 확인합니다.</summary>
public sealed class DataSetManagementServiceTests
{
    [Fact]
    public async Task CreationRejectsInvalidOwnershipBeforeAllocatingStorage()
    {
        var store = new Store();
        store.ScopeValid = false;
        var result = await Service(store).CreateAsync(Scope(), "매출", "parquet");
        Assert.True(result.IsFailure);
        Assert.Equal("dataset.scope_invalid", result.Error.Code);
        Assert.Equal(0, store.Allocations);
    }

    [Fact]
    public async Task CreationPersistsMetadataAfterStorageAllocation()
    {
        var store = new Store();
        var result = await Service(store).CreateAsync(Scope(), "매출", "parquet");
        Assert.True(result.IsSuccess);
        Assert.Single(store.DataSets);
        Assert.Equal(result.Value, store.DataSets[0].Id);
        Assert.Equal(1, store.Allocations);
        Assert.Equal(1, store.Saves);
    }

    [Fact]
    public async Task MetadataFailureReleasesAllocatedStorage()
    {
        var store = new Store { FailSave = true };
        var result = await Service(store).CreateAsync(Scope(), "매출", "parquet");
        Assert.True(result.IsFailure);
        Assert.Equal(1, store.Allocations);
        Assert.Equal(1, store.Releases);
    }

    [Fact]
    public async Task SchemaAndActivationAreTenantScoped()
    {
        var store = new Store();
        var created = await Service(store).CreateAsync(Scope(), "매출", "parquet");
        var id = created.Value;
        var wrongTenant = await Service(store).AddSchemaAsync(id, Guid.NewGuid(), """{"type":"object"}""", DateTimeOffset.UtcNow);
        Assert.True(wrongTenant.IsFailure);
        var schema = await Service(store).AddSchemaAsync(id, store.DataSets[0].Scope.TenantId, """{"type":"object"}""", DateTimeOffset.UtcNow);
        var activated = await Service(store).ActivateAsync(id, store.DataSets[0].Scope.TenantId);
        Assert.True(schema.IsSuccess);
        Assert.Equal(1, schema.Value);
        Assert.True(activated.IsSuccess);
    }

    private static DataSetScope Scope() => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
    private static DataSetManagementService Service(Store store) => new(store, store, store);

    private sealed class Store : IDataSetRepository, IDataSetScopeValidator, IDataSetStorageAdapterResolver, IDataSetStorageAdapter
    {
        public List<DataSetAggregate> DataSets { get; } = [];
        public bool ScopeValid { get; set; } = true;
        public bool FailSave { get; set; }
        public int Allocations { get; private set; }
        public int Releases { get; private set; }
        public int Saves { get; private set; }
        public string AdapterId => "parquet";
        public Task<bool> IsValidAsync(DataSetScope scope, CancellationToken cancellationToken = default) => Task.FromResult(ScopeValid);
        public IDataSetStorageAdapter? Resolve(string adapterId) => adapterId == AdapterId ? this : null;
        public Task<StorageReference> AllocateAsync(DataSetStorageAllocation allocation, CancellationToken cancellationToken = default)
        {
            Allocations++;
            return Task.FromResult(new StorageReference("tenant:01.dataset:" + allocation.DataSetId.Value.ToString("N")));
        }
        public Task ReleaseAsync(StorageReference reference, CancellationToken cancellationToken = default) { Releases++; return Task.CompletedTask; }
        public Task<DataSetAggregate?> GetAsync(DataSetId id, Guid tenantId, CancellationToken cancellationToken = default) =>
            Task.FromResult(DataSets.SingleOrDefault(x => x.Id == id && x.Scope.TenantId == tenantId));
        public void Add(DataSetAggregate dataSet) => DataSets.Add(dataSet);
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            Saves++;
            if (FailSave) throw new InvalidOperationException("저장 실패");
            return Task.FromResult(1);
        }
    }
}
