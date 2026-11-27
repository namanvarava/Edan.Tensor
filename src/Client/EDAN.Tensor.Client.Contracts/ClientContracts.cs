namespace EDAN.Tensor.Client.Contracts;

/// <summary>EDAN Tensor API가 반환하는 안정적인 오류 계약입니다.</summary>
public sealed record ApiErrorContract(string Code, string Description);

/// <summary>현재 인증 주체가 속한 Tenant 소속 정보입니다.</summary>
public sealed record TenantMembershipContract(Guid TenantId, string Role);

/// <summary>서버 페이징을 사용하는 Platform directory 응답입니다.</summary>
public sealed record PlatformPageContract<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record TenantDirectoryContract(Guid Id, string Code, string Name, string Status, DateTimeOffset CreatedAtUtc);
public sealed record OrganizationDirectoryContract(Guid Id, Guid TenantId, string Code, string Name, string Status);
public sealed record ProjectDirectoryContract(Guid Id, Guid TenantId, Guid OrganizationId, string Code, string Name, string Status);
public sealed record UserDirectoryContract(Guid Id, Guid TenantId, string Email, string DisplayName, string Status);
public sealed record InvitationContract(Guid Id, Guid TenantId, Guid UserId, string Email, string DisplayName, string Status,
    DateTimeOffset CreatedAtUtc, DateTimeOffset ExpiresAtUtc, DateTimeOffset? AcceptedAtUtc,
    DateTimeOffset? RevokedAtUtc, int ResendCount);
public sealed record InvitationPageContract(int TotalCount, int Page, int PageSize, IReadOnlyList<InvitationContract> Items);
public sealed record TenantMembershipDirectoryContract(Guid Id, Guid TenantId, string SubjectId, string Role, string Status);
public sealed record OrganizationMembershipDirectoryContract(Guid Id, Guid TenantId, Guid OrganizationId, string SubjectId, string Role, string Status);
public sealed record ProjectMembershipDirectoryContract(Guid Id, Guid TenantId, Guid OrganizationId, Guid ProjectId, string SubjectId, string Role, string Status);

public sealed record TenantMembershipPageContract(
    int TotalCount, int Page, int PageSize, IReadOnlyList<TenantMembershipDirectoryContract> Items);

public sealed record OrganizationMembershipPageContract(
    int TotalCount, int Page, int PageSize, IReadOnlyList<OrganizationMembershipDirectoryContract> Items);

public sealed record ProjectMembershipPageContract(
    int TotalCount, int Page, int PageSize, IReadOnlyList<ProjectMembershipDirectoryContract> Items);

public sealed record AuthorizationPolicyContract(
    Guid Id, Guid? OrganizationId, Guid? ProjectId, string SubjectId, string Action,
    string ResourceType, string Effect, int Priority, bool IsEnabled, DateTimeOffset CreatedAtUtc);

public sealed record AuthorizationPolicyPageContract(
    int TotalCount, int Page, int PageSize, IReadOnlyList<AuthorizationPolicyContract> Items);

public sealed record AuthorizationDecisionContract(bool IsAllowed, string ReasonCode, string? MatchedPolicyId);

public sealed record AuthorizationAuditEntryContract(
    Guid Id, string SubjectId, string Action, string ResourceType, string Scope,
    bool IsAllowed, string ReasonCode, string? MatchedPolicyId, DateTimeOffset OccurredAtUtc);

public sealed record AuthorizationAuditPageContract(
    int TotalCount, int Page, int PageSize, IReadOnlyList<AuthorizationAuditEntryContract> Items);

public sealed record AuthorizationAuditArchiveResultContract(
    Guid ArchiveId, Guid TenantId, DateTimeOffset FromUtc, DateTimeOffset ToUtc,
    int RecordCount, string Sha256, int SizeBytes);

public sealed record AuthorizationAuditArchiveVerificationContract(
    Guid ArchiveId, bool IsValid, string ReasonCode, string? Sha256);

public sealed record AuthorizationAuditArchiveScheduleContract(
    Guid TenantId, bool IsEnabled, int IntervalHours,
    DateTimeOffset? NextRunAtUtc, DateTimeOffset? LastRunAtUtc, long Version);

public sealed record AuthorizationAuditArchiveJobContract(
    Guid JobId, Guid TenantId, DateTimeOffset FromUtc, DateTimeOffset ToUtc,
    string RequestedBy, DateTimeOffset CreatedAtUtc, string Status, Guid? ArchiveId,
    int RecordCount, string? Sha256, int? SizeBytes, DateTimeOffset? CompletedAtUtc,
    string? FailureReason, int AttemptCount, long Version);

public sealed record AuthorizationAuditArchiveAttemptContract(
    Guid Id, int AttemptNumber, string Operation, DateTimeOffset StartedAtUtc,
    DateTimeOffset? FinishedAtUtc, bool? IsSuccessful, string? ResultCode);

public sealed record AuthorizationAuditArchiveJobDetailsContract(
    AuthorizationAuditArchiveJobContract Job,
    IReadOnlyList<AuthorizationAuditArchiveAttemptContract> Attempts);

public sealed record AuthorizationAuditArchiveJobPageContract(
    int TotalCount, int Page, int PageSize, IReadOnlyList<AuthorizationAuditArchiveJobContract> Items);

public sealed record AuthorizationAuditJobVerificationContract(
    Guid JobId, bool IsValid, string ReasonCode, string? Sha256);

public sealed record AuthorizationAuditRetentionPolicyContract(
    Guid TenantId, int RetentionDays, bool IsLegalHold, string? LegalHoldReason,
    string? UpdatedBy, DateTimeOffset? UpdatedAtUtc, long Version);

public sealed record StorageQuotaPolicyContract(
    Guid Id, Guid TenantId, string Scope, Guid TargetId, long? TenantLimitBytes,
    long? OrganizationLimitBytes, long? ProjectLimitBytes, long? DataSetLimitBytes,
    int WarningThresholdPercent, string PlanCode, bool IsEnabled, long Version,
    string UpdatedBy, DateTimeOffset UpdatedAtUtc);

public sealed record StorageQuotaPolicyPageContract(
    int TotalCount, int Page, int PageSize, IReadOnlyList<StorageQuotaPolicyContract> Items);

public sealed record StorageQuotaPolicyHistoryContract(
    Guid Id, Guid TenantId, Guid PolicyId, string ActorSubjectId, string ChangeType,
    string BeforeJson, string AfterJson, DateTimeOffset OccurredAtUtc);

public sealed record StorageQuotaPolicyHistoryPageContract(
    int TotalCount, int Page, int PageSize, IReadOnlyList<StorageQuotaPolicyHistoryContract> Items);

public sealed record DataSetStorageUsageContract(
    long TenantBytes, long TenantDataSetCount, long OrganizationBytes, long OrganizationDataSetCount,
    long ProjectBytes, long ProjectDataSetCount, long DataSetBytes,
    DataSetStorageLimitsContract Limits, DataSetStorageUtilizationContract UtilizationPercent,
    int WarningThresholdPercent, IReadOnlyList<string> Warnings, DateTimeOffset MeasuredAtUtc);

public sealed record DataSetStorageLimitsContract(
    long? TenantLimitBytes, long? OrganizationLimitBytes, long? ProjectLimitBytes, long? DataSetLimitBytes);

public sealed record DataSetStorageUtilizationContract(
    double? Tenant, double? Organization, double? Project, double? DataSet);

public sealed record DataSetTenantStorageUsageContract(long TenantBytes, long TenantDataSetCount, DateTimeOffset MeasuredAtUtc);

public sealed record DataSetStorageUsageSnapshotContract(
    DateTimeOffset PeriodStartUtc, DateTimeOffset CapturedAtUtc, long UsedBytes, long DataSetCount);

public sealed record DataSetQuotaEventContract(
    Guid PolicyId, string Scope, Guid TargetId, string EventType, long UsedBytes, long LimitBytes,
    double UtilizationPercent, long PolicyVersion, DateTimeOffset OccurredAtUtc);

public sealed record DataSetPageContract(
    int TotalCount, int Page, int PageSize, IReadOnlyList<DataSetSummaryContract> Items);

public sealed record DataSetSchemaRevisionPageContract(
    Guid DataSetId, int? CurrentSchemaVersion, IReadOnlyList<DataSetSchemaRevisionContract> Items);
public sealed record DataSetSchemaDiffContract(
    int FromVersion, int ToVersion, string FromSchemaJson, string ToSchemaJson, bool Changed);

public sealed record ExternalDeliveryContract(
    Guid Id, Guid TenantId, string TargetUri, string IdempotencyKey, string Status,
    int AttemptCount, int MaxAttempts, string? LastErrorCode,
    DateTimeOffset CreatedAtUtc, DateTimeOffset? CompletedAtUtc);

public sealed record ExternalDeliveryPageContract(
    int TotalCount, int Page, int PageSize, IReadOnlyList<ExternalDeliveryContract> Items);

public sealed record CreateExternalDeliveryResponseContract(
    ExternalDeliveryContract Delivery, Guid WorkerTaskId, bool Created);

public sealed record ResourceIdContract(Guid Id);
public sealed record CreateTenantRequestContract(string Code, string Name);
public sealed record CreateOrganizationRequestContract(string Code, string Name);
public sealed record CreateProjectRequestContract(string Code, string Name);
public sealed record InviteUserRequestContract(string Email, string DisplayName);
public sealed record InvitationLifetimeRequestContract(int? LifetimeDays = null);
public sealed record UpdateDirectoryEntryRequestContract(string? Name, string? Status);
public sealed record SetUserStatusRequestContract(string Status);
public sealed record GrantTenantMembershipRequestContract(string SubjectId, string Role);
public sealed record GrantOrganizationMembershipRequestContract(string SubjectId, string Role);
public sealed record GrantProjectMembershipRequestContract(string SubjectId, string Role);
public sealed record UpdateMembershipRequestContract(string? Role, bool Revoke = false);
public sealed record CreateAuthorizationPolicyRequestContract(
    Guid? OrganizationId, Guid? ProjectId, string SubjectId, string Action,
    string ResourceType, string Effect, int Priority);
public sealed record EvaluateAuthorizationPolicyRequestContract(
    Guid? OrganizationId, Guid? ProjectId, string? SubjectId, string Action, string ResourceType);
public sealed record UpdateAuthorizationPolicyRequestContract(string Effect, int Priority);
public sealed record SetAuthorizationPolicyEnabledRequestContract(bool IsEnabled);
public sealed record CreateStorageQuotaPolicyRequestContract(
    string Scope, Guid TargetId, long? TenantLimitBytes, long? OrganizationLimitBytes,
    long? ProjectLimitBytes, long? DataSetLimitBytes, int WarningThresholdPercent = 80,
    string? PlanCode = "Community");
public sealed record UpdateStorageQuotaPolicyRequestContract(
    long ExpectedVersion, long? TenantLimitBytes, long? OrganizationLimitBytes,
    long? ProjectLimitBytes, long? DataSetLimitBytes, int WarningThresholdPercent,
    string PlanCode);
public sealed record SetStorageQuotaPolicyEnabledRequestContract(bool IsEnabled, long ExpectedVersion);
public sealed record CreateDataSetValidationProfileRequestContract(string Name, string DefinitionJson);
public sealed record CreateDataSetValidationRevisionRequestContract(string DefinitionJson);
public sealed record UpdateDataSetValidationProfileRequestContract(string Name);
public sealed record CreateExternalDeliveryRequestContract(
    string TargetUri, System.Text.Json.JsonElement Payload, int MaxAttempts = 5);
public sealed record VerifyAuthorizationAuditIntegrityRequestContract(
    DateTimeOffset FromUtc, DateTimeOffset ToUtc, string ExpectedSha256);
public sealed record CreateAuthorizationAuditArchiveRequestContract(DateTimeOffset FromUtc, DateTimeOffset ToUtc);
public sealed record VerifyAuthorizationAuditArchiveRequestContract(string? ExpectedSha256);
public sealed record UpdateAuthorizationAuditArchiveScheduleRequestContract(int IntervalHours, bool IsEnabled);
public sealed record UpdateAuthorizationAuditRetentionRequestContract(
    int RetentionDays, bool IsLegalHold, string? LegalHoldReason);
public sealed record CreateDataSetValidationProfileContract(
    Guid Id, Guid TenantId, string Name, int Revision, string DefinitionJson, DateTimeOffset CreatedAtUtc);
public sealed record CreateDataSetValidationProfileRevisionContract(
    Guid Id, Guid TenantId, string Name, int Revision, string DefinitionJson, DateTimeOffset CreatedAtUtc);
public sealed record AuthorizationAuditIntegrityResultContract(
    Guid TenantId, DateTimeOffset FromUtc, DateTimeOffset ToUtc, int RecordCount, string Sha256);
public sealed record AuthorizationAuditIntegrityVerificationContract(
    AuthorizationAuditIntegrityResultContract Current, bool IsValid);

/// <summary>DataSet 생성을 요청하는 입력입니다.</summary>
public sealed record CreateDataSetRequestContract(
    Guid OrganizationId,
    Guid ProjectId,
    string Name,
    string StorageAdapter = "sqlite",
    string ContentFormat = "json-array");

public sealed record SystemMenuContractV1(Guid Id, Guid TenantId, string Code, string Name, string Path, bool IsEnabled);
public sealed record UpsertSystemMenuRequestV1(string Code, string Name, string Path, bool IsEnabled = true);
public sealed record SystemMenuPageContractV1(IReadOnlyList<SystemMenuContractV1> Items, int TotalCount, int Page, int PageSize);
public sealed record MasterCodeContractV1(Guid Id, Guid TenantId, string GroupCode, string Code, string Name, int SortOrder, bool IsEnabled);
public sealed record UpsertMasterCodeRequestV1(string GroupCode, string Code, string Name, int SortOrder = 0, bool IsEnabled = true);
public sealed record MasterCodePageContractV1(IReadOnlyList<MasterCodeContractV1> Items, int TotalCount, int Page, int PageSize);
public sealed record BusinessWorkItemContractV1(Guid Id, Guid TenantId, string WorkType, string ReferenceNo, string Status, DateTimeOffset CreatedAtUtc);
public sealed record CreateBusinessWorkItemRequestV1(string WorkType, string ReferenceNo);
public sealed record BusinessWorkItemPageContractV1(IReadOnlyList<BusinessWorkItemContractV1> Items, int TotalCount, int Page, int PageSize);

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

public sealed record DataSetSummaryContract(
    Guid Id, Guid TenantId, Guid OrganizationId, Guid ProjectId, string Name,
    string StorageAdapter, string ContentFormat, string Status, int Version,
    DateTimeOffset CreatedAtUtc, int? CurrentSchemaVersion);

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

public sealed record DataSetValidationProfilePageContract(
    int TotalCount, int Page, int PageSize, IReadOnlyList<CreateDataSetValidationProfileContract> Items);

public sealed record DataSetValidationExecutionPageContract(
    int TotalCount, int Page, int PageSize, IReadOnlyList<DataSetValidationExecutionContract> Items);

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

public sealed record WorkerTaskStatusPageContract(
    int TotalCount, int Page, int PageSize, IReadOnlyList<WorkerTaskStatusContract> Items);
public sealed record WorkerTaskResultContract(Guid TaskId, string TaskType, string Status, string ResultReference);

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
    int TotalCount,
    int Page,
    int PageSize,
    IReadOnlyList<WorkerDeadLetterContract> Items);

/// <summary>Worker DLQ 재처리 결과입니다.</summary>
public sealed record ReplayWorkerDeadLetterResponseContract(
    Guid OriginalTaskId,
    Guid ReplayTaskId);

public sealed record WorkerReplayHistoryContract(
    Guid Id, Guid OriginalTaskId, Guid ReplayTaskId, Guid ReplayRequestId,
    string RequestedBy, DateTimeOffset CreatedAtUtc);

public sealed record WorkerReplayHistoryPageContract(
    int TotalCount, int Page, int PageSize, IReadOnlyList<WorkerReplayHistoryContract> Items);
