namespace EDAN.Tensor.Client.Contracts;

/// <summary>EDAN Tensor API가 반환하는 안정적인 오류 계약입니다.</summary>
public sealed record ApiErrorContract(string Code, string Description);

/// <summary>현재 인증 주체가 속한 Tenant 소속 정보입니다.</summary>
public sealed record TenantMembershipContract(Guid TenantId, string Role);

/// <summary>DataSet 생성을 요청하는 입력입니다.</summary>
public sealed record CreateDataSetRequestContract(
    Guid OrganizationId,
    Guid ProjectId,
    string Name,
    string StorageAdapter = "sqlite",
    string ContentFormat = "json-array");

/// <summary>DataSet 생성 직후 반환되는 기본 정보입니다.</summary>
public sealed record CreateDataSetResponseContract(
    Guid Id,
    Guid TenantId,
    Guid OrganizationId,
    Guid ProjectId,
    string Name,
    string StorageAdapter,
    string ContentFormat,
    string Status);

/// <summary>DataSet의 불변 스키마 revision 정보입니다.</summary>
public sealed record DataSetSchemaRevisionContract(
    int Version,
    string SchemaJson,
    DateTimeOffset CreatedAtUtc);

/// <summary>DataSet의 Tenant 범위 상세 정보입니다.</summary>
public sealed record DataSetDetailsContract(
    Guid Id,
    Guid TenantId,
    Guid OrganizationId,
    Guid ProjectId,
    string Name,
    string StorageAdapter,
    string ContentFormat,
    string Status,
    int Version,
    DateTimeOffset CreatedAtUtc,
    int? CurrentSchemaVersion,
    IReadOnlyList<DataSetSchemaRevisionContract> SchemaRevisions);

/// <summary>DataSet 스키마 revision 등록 요청입니다.</summary>
public sealed record AddDataSetSchemaRequestContract(string SchemaJson);

/// <summary>DataSet 스키마 등록 결과입니다.</summary>
public sealed record AddDataSetSchemaResponseContract(Guid DataSetId, int SchemaVersion);

/// <summary>DataSet 검증 실행 등록 요청입니다.</summary>
public sealed record CreateDataSetValidationRequestContract(
    int ExpectedDataSetVersion,
    int SchemaVersion,
    Guid ProfileId,
    int ProfileRevision);

/// <summary>DataSet 검증 실행의 API 상태 요약입니다.</summary>
public sealed record DataSetValidationExecutionContract(
    Guid Id,
    Guid TenantId,
    Guid DataSetId,
    int ExpectedDataSetVersion,
    int SchemaVersion,
    Guid ProfileId,
    int ProfileRevision,
    long FencingToken,
    string Status,
    string DetailReference,
    int ProcessedRecords,
    int TotalRecords,
    int ErrorCount,
    int WarningCount,
    int DetailItemCount,
    bool DetailTruncated,
    string RuleCountsJson,
    string? FailureCode,
    string? FailureMessage,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc);

/// <summary>검증 실행 등록 결과와 연결된 Worker 작업 식별자입니다.</summary>
public sealed record CreateDataSetValidationResponseContract(
    DataSetValidationExecutionContract Execution,
    Guid WorkerTaskId,
    bool Created);

/// <summary>검증 결과 상세 항목입니다.</summary>
public sealed record DataSetValidationDetailContract(
    int RowNumber,
    string? FieldName,
    string Code,
    string Severity,
    string Message);

/// <summary>검증 결과 상세 페이지입니다.</summary>
public sealed record DataSetValidationDetailsPageContract(
    int Skip,
    int Take,
    IReadOnlyList<DataSetValidationDetailContract> Items);

/// <summary>Worker 작업 등록 요청입니다.</summary>
public sealed record EnqueueWorkerTaskRequestContract(
    string TaskType,
    string PayloadReference,
    int MaxAttempts = 5);

/// <summary>Worker 작업 등록 결과입니다.</summary>
public sealed record EnqueueWorkerTaskResponseContract(
    Guid Id,
    string TaskType,
    string Status,
    DateTimeOffset CreatedAtUtc);

/// <summary>Worker 작업의 안전한 운영 상태입니다.</summary>
public sealed record WorkerTaskStatusContract(
    Guid Id,
    string TaskType,
    string Status,
    int AttemptCount,
    int MaxAttempts,
    long FencingToken,
    long CheckpointVersion,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? NextAttemptAtUtc,
    DateTimeOffset? CompletedAtUtc,
    string? LastErrorCode,
    DateTimeOffset? LeaseExpiresAtUtc,
    int ProgressPercent,
    string? ProgressStage,
    string? ProgressMessage,
    bool HasResult);

/// <summary>Worker DLQ 항목의 운영 메타데이터입니다.</summary>
public sealed record WorkerDeadLetterContract(
    Guid Id,
    string TaskType,
    string Status,
    int AttemptCount,
    int MaxAttempts,
    string? LastErrorCode,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? NextAttemptAtUtc);

/// <summary>Worker DLQ 페이지입니다.</summary>
public sealed record WorkerDeadLetterPageContract(
    int Page,
    int PageSize,
    IReadOnlyList<WorkerDeadLetterContract> Items);

/// <summary>Worker DLQ 재처리 결과입니다.</summary>
public sealed record ReplayWorkerDeadLetterResponseContract(
    Guid OriginalTaskId,
    Guid ReplayTaskId);
