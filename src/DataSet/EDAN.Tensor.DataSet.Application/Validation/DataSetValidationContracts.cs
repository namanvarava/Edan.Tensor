namespace EDAN.Tensor.DataSet.Application.Validation;

/// <summary>DataSet 검증 Worker 작업의 유형과 불투명 Payload 참조를 정의합니다.</summary>
public static class DataSetValidationTaskContract
{
    public const string TaskType = "dataset.validation.v1";

    public static string CreatePayloadReference(Guid executionId)
    {
        if (executionId == Guid.Empty) throw new ArgumentException("실행 식별자는 비어 있을 수 없습니다.", nameof(executionId));
        return $"dataset-validation:v1:{executionId:N}";
    }

    public static bool TryParsePayloadReference(string payloadReference, out Guid executionId)
    {
        executionId = default;
        if (string.IsNullOrWhiteSpace(payloadReference)) return false;
        const string prefix = "dataset-validation:v1:";
        return payloadReference.StartsWith(prefix, StringComparison.Ordinal)
            && Guid.TryParseExact(payloadReference[prefix.Length..], "N", out executionId)
            && executionId != Guid.Empty;
    }
}

/// <summary>검증 프로필에서 허용하는 값 형식입니다.</summary>
public enum DataSetValidationFieldType { String, Integer, Decimal, Boolean }

/// <summary>검증 규칙이 위반되었을 때 결과에 기록할 심각도입니다.</summary>
public enum DataSetValidationSeverity { Error, Warning }

/// <summary>검증 실행의 영속 상태입니다.</summary>
public enum DataSetValidationStatus { Queued, Running, Valid, Invalid, Failed }

/// <summary>프로필 필드 규칙을 나타냅니다.</summary>
public sealed record DataSetValidationFieldRule(
    string Name,
    DataSetValidationFieldType Type,
    bool Required,
    DataSetValidationSeverity Severity,
    string RequiredCode,
    string TypeCode,
    int? MinLength,
    int? MaxLength,
    decimal? Min,
    decimal? Max);

/// <summary>여러 필드의 조합 중복 규칙을 나타냅니다.</summary>
public sealed record DataSetValidationDuplicateRule(
    IReadOnlyList<string> Fields,
    DataSetValidationSeverity Severity,
    string Code);

/// <summary>검증 프로필 문서에서 해석한 규칙과 자원 제한입니다.</summary>
public sealed record DataSetValidationProfileDefinition(
    IReadOnlyList<DataSetValidationFieldRule> Fields,
    IReadOnlyList<DataSetValidationDuplicateRule> DuplicateSets,
    int MaxRecords,
    int MaxFieldLength,
    int MaxDetailItems);

/// <summary>한 행에서 발견한 오류 또는 경고이며 원문 값을 포함하지 않습니다.</summary>
public sealed record DataSetValidationFinding(
    string Code,
    DataSetValidationSeverity Severity,
    string? FieldName,
    string Message);

/// <summary>중복 검증에 사용할 해시 키이며 원문 값을 보관하지 않습니다.</summary>
public sealed record DataSetValidationDuplicateObservation(string RuleCode, string KeyHash);

/// <summary>검증 프로필과 불변 revision의 조회 결과입니다.</summary>
public sealed record DataSetValidationProfileSnapshot(
    Guid Id,
    Guid TenantId,
    string Name,
    int Revision,
    string DefinitionJson,
    DateTimeOffset CreatedAtUtc);

/// <summary>검증 실행의 요약과 현재 진행 위치입니다.</summary>
public sealed record DataSetValidationExecutionSnapshot(
    Guid Id,
    Guid TenantId,
    Guid DataSetId,
    int ExpectedDataSetVersion,
    int SchemaVersion,
    Guid ProfileId,
    int ProfileRevision,
    DataSetValidationStatus Status,
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

/// <summary>API와 Worker가 공유하는 검증 실행 요청입니다.</summary>
public sealed record DataSetValidationRequest(
    int ExpectedDataSetVersion,
    int SchemaVersion,
    Guid ProfileId,
    int ProfileRevision);

/// <summary>검증 결과에서 노출 가능한 상세 항목입니다.</summary>
public sealed record DataSetValidationDetailSnapshot(
    int RowNumber,
    string? FieldName,
    string Code,
    DataSetValidationSeverity Severity,
    string Message);

/// <summary>검증 실행의 종료 방식입니다.</summary>
public enum DataSetValidationRunOutcome { Completed, QueuedForRetry, AlreadyCompleted }
