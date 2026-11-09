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

    /// <summary>검증 실행을 저장하고 동시 중복 요청은 기존 키의 실행으로 수렴시킵니다.</summary>
    Task<DataSetValidationExecutionSnapshot> CreateExecutionAsync(
        Guid tenantId, Guid dataSetId, int expectedDataSetVersion, int schemaVersion,
        Guid profileId, int profileRevision, DateTimeOffset nowUtc,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>Tenant와 Idempotency-Key로 기존 검증 실행을 조회합니다.</summary>
    Task<DataSetValidationExecutionSnapshot?> GetExecutionByIdempotencyKeyAsync(
        Guid tenantId, string idempotencyKey, CancellationToken cancellationToken = default);

    Task<DataSetValidationExecutionSnapshot?> GetExecutionAsync(
        Guid tenantId, Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>현재 Worker Fencing Token으로 검증 실행의 소유권을 시작합니다.</summary>
    Task<bool> TryStartAsync(
        Guid tenantId, Guid executionId, long fencingToken,
        CancellationToken cancellationToken = default);

    Task<IReadOnlySet<string>> GetSeenKeysAsync(
        Guid executionId, CancellationToken cancellationToken = default);

    Task<bool> RecordRowAsync(
        Guid tenantId, Guid executionId, long fencingToken, int rowNumber,
        IReadOnlyList<DataSetValidationFinding> findings,
        IReadOnlyList<DataSetValidationFinding> detailFindings,
        IReadOnlyList<DataSetValidationDuplicateObservation> duplicateObservations,
        int processedRecords, int totalRecords, bool detailTruncated,
        CancellationToken cancellationToken = default);

    /// <summary>현재 Fencing Token이 유효할 때 검증 실행을 완료합니다.</summary>
    Task<bool> CompleteAsync(
        Guid tenantId, Guid executionId, long fencingToken, DataSetValidationStatus status,
        int totalRecords, bool detailTruncated, string ruleCountsJson,
        CancellationToken cancellationToken = default);

    /// <summary>현재 Fencing Token이 유효할 때 검증 실행을 재시도 대기로 되돌립니다.</summary>
    Task<bool> MarkQueuedAsync(
        Guid tenantId, Guid executionId, long fencingToken,
        CancellationToken cancellationToken = default);

    /// <summary>현재 Fencing Token이 유효할 때 검증 실행을 실패로 종료합니다.</summary>
    Task<bool> FailAsync(
        Guid tenantId, Guid executionId, long fencingToken, string code, string message,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataSetValidationDetailSnapshot>> GetDetailsAsync(
        Guid tenantId, Guid executionId, int skip, int take,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteExecutionAsync(
        Guid tenantId, Guid executionId, CancellationToken cancellationToken = default);

    /// <summary>Worker 등록 전에 생성된 미실행 검증 실행을 내부 보상으로 제거합니다.</summary>
    Task<bool> DeleteUnqueuedExecutionAsync(
        Guid tenantId, Guid executionId, CancellationToken cancellationToken = default);
}

/// <summary>검증 실행에 저장할 한 행의 오류·경고 결과입니다.</summary>
public sealed record DataSetValidationRowWrite(
    int RowNumber,
    IReadOnlyList<DataSetValidationFinding> Findings,
    IReadOnlyList<DataSetValidationDuplicateObservation> DuplicateObservations);
