using EDAN.Tensor.DataSet.Application.Validation;

namespace EDAN.Tensor.DataSet.Application.Abstractions;

/// <summary>검증 프로필과 실행 결과를 Tenant 범위로 영속화하는 계약입니다.</summary>
public interface IDataSetValidationRepository
{
    Task<DataSetValidationProfileSnapshot?> GetProfileAsync(
        Guid tenantId, Guid profileId, int revision, CancellationToken cancellationToken = default);

    Task<DataSetValidationProfileSnapshot> CreateProfileAsync(
        Guid tenantId, string name, string definitionJson, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);

    Task<DataSetValidationProfileSnapshot> AddProfileRevisionAsync(
        Guid tenantId, Guid profileId, string definitionJson, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);

    Task<DataSetValidationExecutionSnapshot> CreateExecutionAsync(
        Guid tenantId, Guid dataSetId, int expectedDataSetVersion, int schemaVersion,
        Guid profileId, int profileRevision, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);

    Task<DataSetValidationExecutionSnapshot?> GetExecutionAsync(
        Guid tenantId, Guid executionId, CancellationToken cancellationToken = default);

    Task<bool> TryStartAsync(
        Guid tenantId, Guid executionId, CancellationToken cancellationToken = default);

    Task<IReadOnlySet<string>> GetSeenKeysAsync(
        Guid executionId, CancellationToken cancellationToken = default);

    Task<bool> RecordRowAsync(
        Guid tenantId, Guid executionId, int rowNumber,
        IReadOnlyList<DataSetValidationFinding> findings,
        IReadOnlyList<DataSetValidationFinding> detailFindings,
        IReadOnlyList<DataSetValidationDuplicateObservation> duplicateObservations,
        int processedRecords, int totalRecords, bool detailTruncated,
        CancellationToken cancellationToken = default);

    Task CompleteAsync(
        Guid tenantId, Guid executionId, DataSetValidationStatus status,
        int totalRecords, bool detailTruncated, string ruleCountsJson,
        CancellationToken cancellationToken = default);

    Task MarkQueuedAsync(
        Guid tenantId, Guid executionId, CancellationToken cancellationToken = default);

    Task FailAsync(
        Guid tenantId, Guid executionId, string code, string message,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataSetValidationDetailSnapshot>> GetDetailsAsync(
        Guid tenantId, Guid executionId, int skip, int take,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteExecutionAsync(
        Guid tenantId, Guid executionId, CancellationToken cancellationToken = default);
}

/// <summary>검증 실행에 저장할 한 행의 오류·경고 결과입니다.</summary>
public sealed record DataSetValidationRowWrite(
    int RowNumber,
    IReadOnlyList<DataSetValidationFinding> Findings,
    IReadOnlyList<DataSetValidationDuplicateObservation> DuplicateObservations);
