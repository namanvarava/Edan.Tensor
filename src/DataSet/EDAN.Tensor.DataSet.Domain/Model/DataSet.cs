using System.ComponentModel.DataAnnotations.Schema;
using EDAN.Tensor.SharedKernel.Exceptions;
using EDAN.Tensor.SharedKernel.Primitives;

namespace EDAN.Tensor.DataSet.Domain.Model;

/// <summary>테넌트·조직·프로젝트 경계 안에서 데이터 자산의 메타데이터와 수명 주기를 관리합니다.</summary>
public sealed class DataSet : AggregateRoot<DataSetId>
{
    private readonly List<DataSetSchemaRevision> _schemaRevisions = [];

    // EF Core는 소유 객체(Navigation)를 생성자 인자로 바인딩할 수 없으므로 스칼라 속성만 받습니다.
    private DataSet(DataSetId id, string name, string storageAdapter)
        : base(id)
    {
        Name = ValidateName(name);
        StorageAdapter = ValidateAdapter(storageAdapter);
        Status = DataSetStatus.Draft;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        Version = 1;
    }

    [NotMapped]
    public DataSetScope Scope => new(TenantId, OrganizationId, ProjectId);
    // 복합 객체 속성은 EF 인덱스 키로 직접 사용할 수 없어 조회·인덱싱용 스칼라 키를 함께 유지합니다.
    public Guid TenantId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; }
    public StorageReference StorageReference { get; private set; } = null!;
    public string StorageAdapter { get; }
    public DataSetStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public int Version { get; private set; }
    public IReadOnlyList<DataSetSchemaRevision> SchemaRevisions => _schemaRevisions.AsReadOnly();
    public int? CurrentSchemaVersion => _schemaRevisions.Count == 0 ? null : _schemaRevisions[^1].Version;

    /// <summary>필수 소유 범위와 저장소 참조를 지정하여 DataSet을 생성합니다.</summary>
    public static DataSet Create(
        DataSetId id,
        DataSetScope scope,
        string name,
        StorageReference storageReference,
        string storageAdapter)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(storageReference);
        var dataSet = new DataSet(id, name, storageAdapter)
        {
            TenantId = scope.TenantId,
            OrganizationId = scope.OrganizationId,
            ProjectId = scope.ProjectId,
            StorageReference = storageReference
        };
        return dataSet;
    }

    /// <summary>JSON Schema 문서를 불변 버전으로 추가합니다.</summary>
    public DataSetSchemaRevision AddSchema(string schemaJson, DateTimeOffset createdAtUtc)
    {
        EnsureNotArchived();
        var revision = DataSetSchemaRevision.Create((_schemaRevisions.Count == 0 ? 0 : _schemaRevisions[^1].Version) + 1,
            schemaJson, createdAtUtc);
        _schemaRevisions.Add(revision);
        Version++;
        return revision;
    }

    /// <summary>DataSet 표시 이름을 변경합니다.</summary>
    public void Rename(string name)
    {
        EnsureNotArchived();
        Name = ValidateName(name);
        Version++;
    }

    /// <summary>저장 위치를 불투명 참조로 교체합니다.</summary>
    public void ChangeStorageReference(StorageReference storageReference)
    {
        EnsureNotArchived();
        ArgumentNullException.ThrowIfNull(storageReference);
        StorageReference = storageReference;
        Version++;
    }

    /// <summary>스키마가 등록된 DataSet을 활성화합니다.</summary>
    public void Activate()
    {
        if (Status != DataSetStatus.Draft)
            throw new BusinessRuleViolationException("dataset.invalid_activation", "초안 상태의 DataSet만 활성화할 수 있습니다.");
        if (_schemaRevisions.Count == 0)
            throw new BusinessRuleViolationException("dataset.schema_required", "DataSet을 활성화하려면 스키마가 필요합니다.");
        Status = DataSetStatus.Active;
        Version++;
    }

    /// <summary>활성 DataSet을 보관 상태로 전환합니다.</summary>
    public void Archive()
    {
        if (Status != DataSetStatus.Active)
            throw new BusinessRuleViolationException("dataset.invalid_archive", "활성 상태의 DataSet만 보관할 수 있습니다.");
        Status = DataSetStatus.Archived;
        Version++;
    }

    private void EnsureNotArchived()
    {
        if (Status == DataSetStatus.Archived)
            throw new BusinessRuleViolationException("dataset.archived", "보관된 DataSet은 변경할 수 없습니다.");
    }

    private static string ValidateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var value = name.Trim();
        if (value.Length > 160)
            throw new ArgumentException("DataSet 이름은 160자를 초과할 수 없습니다.", nameof(name));
        return value;
    }

    private static string ValidateAdapter(string adapter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(adapter);
        var value = adapter.Trim().ToLowerInvariant();
        if (value.Length > 64 || value.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '.' or '-' or '_')))
            throw new ArgumentException("저장소 어댑터 식별자 형식이 올바르지 않습니다.", nameof(adapter));
        return value;
    }
}

/// <summary>DataSet의 필수 소유 경계를 나타냅니다. 플랫폼 도메인 객체를 참조하지 않습니다.</summary>
public sealed record DataSetScope
{
    public DataSetScope(Guid tenantId, Guid organizationId, Guid projectId)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        if (organizationId == Guid.Empty) throw new ArgumentException("Organization 식별자는 비어 있을 수 없습니다.", nameof(organizationId));
        if (projectId == Guid.Empty) throw new ArgumentException("Project 식별자는 비어 있을 수 없습니다.", nameof(projectId));
        TenantId = tenantId;
        OrganizationId = organizationId;
        ProjectId = projectId;
    }

    public Guid TenantId { get; }
    public Guid OrganizationId { get; }
    public Guid ProjectId { get; }
}

/// <summary>DataSet 레코드의 전역 식별자입니다.</summary>
public sealed record DataSetId : StronglyTypedId<Guid>
{
    public DataSetId(Guid value) : base(value == Guid.Empty
        ? throw new ArgumentException("DataSet 식별자는 비어 있을 수 없습니다.", nameof(value))
        : value) { }
}

/// <summary>DataSet의 수명 주기 상태입니다.</summary>
public enum DataSetStatus { Draft, Active, Archived }
