using DataSetAggregate = EDAN.Tensor.DataSet.Domain.Model.DataSet;
using EDAN.Tensor.DataSet.Domain.Model;
using EDAN.Tensor.SharedKernel.Exceptions;
using Xunit;

namespace EDAN.Tensor.DataSet.Domain.Tests;

/// <summary>DataSet 소유 경계, 스키마 버전, 저장소 참조 및 수명 주기 규칙을 검증합니다.</summary>
public sealed class DataSetDomainTests
{
    private static DataSetAggregate Create() => DataSetAggregate.Create(
        new DataSetId(Guid.NewGuid()),
        new DataSetScope(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
        "  고객 데이터  ",
        new StorageReference("tenant:01.dataset:01"),
        "parquet");

    [Fact]
    public void CreationRequiresFullOwnershipAndOpaqueStorageReference()
    {
        var dataSet = Create();
        Assert.Equal(DataSetStatus.Draft, dataSet.Status);
        Assert.Equal("고객 데이터", dataSet.Name);
        Assert.Equal("parquet", dataSet.StorageAdapter);
        Assert.Throws<ArgumentException>(() => new DataSetScope(Guid.Empty, Guid.NewGuid(), Guid.NewGuid()));
        Assert.Throws<ArgumentException>(() => new StorageReference("../private/file.parquet"));
        Assert.Throws<ArgumentException>(() => new StorageReference("https://host/data"));
    }

    [Fact]
    public void SchemaRevisionsAreSequentialAndImmutable()
    {
        var dataSet = Create();
        var first = dataSet.AddSchema("""{"type":"object","properties":{"id":{"type":"string"}}}""", DateTimeOffset.UtcNow);
        var second = dataSet.AddSchema("""{"type":"object","required":["id"]}""", DateTimeOffset.UtcNow);
        Assert.Equal(1, first.Version);
        Assert.Equal(2, second.Version);
        Assert.Equal(2, dataSet.CurrentSchemaVersion);
        Assert.Equal(2, dataSet.SchemaRevisions.Count);
    }

    [Fact]
    public void InvalidSchemaDocumentsAreRejected()
    {
        Assert.ThrowsAny<System.Text.Json.JsonException>(() => DataSetSchemaRevision.Create(1, "{invalid", DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentException>(() => DataSetSchemaRevision.Create(1, "[]", DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentOutOfRangeException>(() => DataSetSchemaRevision.Create(0, "{}", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void DatasetMustHaveSchemaBeforeActivation()
    {
        var dataSet = Create();
        Assert.Throws<BusinessRuleViolationException>(() => dataSet.Activate());
        dataSet.AddSchema("""{"type":"object"}""", DateTimeOffset.UtcNow);
        dataSet.Activate();
        Assert.Equal(DataSetStatus.Active, dataSet.Status);
    }

    [Fact]
    public void ArchivedDatasetCannotBeChanged()
    {
        var dataSet = Create();
        dataSet.AddSchema("""{"type":"object"}""", DateTimeOffset.UtcNow);
        dataSet.Activate();
        dataSet.Archive();
        Assert.Throws<BusinessRuleViolationException>(() => dataSet.Rename("변경"));
        Assert.Throws<BusinessRuleViolationException>(() => dataSet.AddSchema("""{"type":"array"}""", DateTimeOffset.UtcNow));
        Assert.Throws<BusinessRuleViolationException>(() => dataSet.ChangeStorageReference(new("tenant:01.dataset:02")));
    }

    [Fact]
    public void MetadataChangesAdvanceOptimisticVersion()
    {
        var dataSet = Create();
        Assert.Equal(1, dataSet.Version);
        dataSet.Rename("변경된 이름");
        dataSet.ChangeStorageReference(new("tenant:01.dataset:02"));
        dataSet.AddSchema("""{"type":"object"}""", DateTimeOffset.UtcNow);
        Assert.Equal(4, dataSet.Version);
    }
}
