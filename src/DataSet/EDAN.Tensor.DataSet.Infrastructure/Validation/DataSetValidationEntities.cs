namespace EDAN.Tensor.DataSet.Infrastructure.Validation;

/// <summary>Tenant가 소유한 검증 프로필의 기본 정보를 저장합니다.</summary>
public sealed class DataSetValidationProfile
{
    private DataSetValidationProfile() { }

    public DataSetValidationProfile(Guid id, Guid tenantId, string name, DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty) throw new ArgumentException("검증 프로필 식별자는 비어 있을 수 없습니다.", nameof(id));
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        Name = ValidateText(name, 160, nameof(name));
        Id = id;
        TenantId = tenantId;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private static string ValidateText(string value, int maxLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maxLength)
            throw new ArgumentException($"값은 {maxLength}자를 초과할 수 없습니다.", parameterName);
        return normalized;
    }
}

/// <summary>검증 프로필의 불변 revision을 저장합니다.</summary>
public sealed class DataSetValidationProfileRevision
{
    private DataSetValidationProfileRevision() { }

    public DataSetValidationProfileRevision(Guid profileId, Guid tenantId, int revision,
        string definitionJson, DateTimeOffset createdAtUtc)
    {
        if (profileId == Guid.Empty) throw new ArgumentException("검증 프로필 식별자는 비어 있을 수 없습니다.", nameof(profileId));
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        if (revision <= 0) throw new ArgumentOutOfRangeException(nameof(revision));
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionJson);
        if (definitionJson.Length > 1_048_576)
            throw new ArgumentException("검증 프로필 문서는 1 MiB를 초과할 수 없습니다.", nameof(definitionJson));
        ProfileId = profileId;
        TenantId = tenantId;
        Revision = revision;
        DefinitionJson = definitionJson;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }

    public Guid ProfileId { get; private set; }
    public Guid TenantId { get; private set; }
    public int Revision { get; private set; }
    public string DefinitionJson { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
}

/// <summary>DataSet 검증 실행과 요약 결과를 저장합니다.</summary>
public sealed class DataSetValidationExecution
{
    private DataSetValidationExecution() { }

    public DataSetValidationExecution(Guid id, Guid tenantId, Guid dataSetId,
        int expectedDataSetVersion, int schemaVersion, Guid profileId, int profileRevision,
        string detailReference, DateTimeOffset createdAtUtc, string? idempotencyKey = null)
    {
        if (id == Guid.Empty) throw new ArgumentException("실행 식별자는 비어 있을 수 없습니다.", nameof(id));
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        if (dataSetId == Guid.Empty) throw new ArgumentException("DataSet 식별자는 비어 있을 수 없습니다.", nameof(dataSetId));
        if (expectedDataSetVersion <= 0) throw new ArgumentOutOfRangeException(nameof(expectedDataSetVersion));
        if (schemaVersion <= 0) throw new ArgumentOutOfRangeException(nameof(schemaVersion));
        if (profileId == Guid.Empty) throw new ArgumentException("검증 프로필 식별자는 비어 있을 수 없습니다.", nameof(profileId));
        if (profileRevision <= 0) throw new ArgumentOutOfRangeException(nameof(profileRevision));
        ArgumentException.ThrowIfNullOrWhiteSpace(detailReference);
        if (detailReference.Trim().Length > 128)
            throw new ArgumentException("상세 결과 참조는 128자를 초과할 수 없습니다.", nameof(detailReference));
        Id = id;
        TenantId = tenantId;
        DataSetId = dataSetId;
        ExpectedDataSetVersion = expectedDataSetVersion;
        SchemaVersion = schemaVersion;
        ProfileId = profileId;
        ProfileRevision = profileRevision;
        DetailReference = detailReference.Trim();
        IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey)
            ? null
            : ValidateText(idempotencyKey, 200, nameof(idempotencyKey));
        Status = "Queued";
        RuleCountsJson = "{}";
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        Version = 1;
        FencingToken = 0;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid DataSetId { get; private set; }
    public int ExpectedDataSetVersion { get; private set; }
    public int SchemaVersion { get; private set; }
    public Guid ProfileId { get; private set; }
    public int ProfileRevision { get; private set; }
    /// <summary>같은 Tenant의 중복 검증 예약을 하나로 수렴시키는 선택적 키입니다.</summary>
    public string? IdempotencyKey { get; private set; }
    public long FencingToken { get; private set; }
    public string Status { get; private set; } = "Queued";
    public string DetailReference { get; private set; } = string.Empty;
    public int ProcessedRecords { get; private set; }
    public int TotalRecords { get; private set; }
    public int ErrorCount { get; private set; }
    public int WarningCount { get; private set; }
    public int DetailItemCount { get; private set; }
    public bool DetailTruncated { get; private set; }
    public string RuleCountsJson { get; private set; } = "{}";
    public string? FailureCode { get; private set; }
    public string? FailureMessage { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public long Version { get; private set; }

    /// <summary>현재 Worker 실행의 Fencing Token을 기록하여 오래된 실행의 결과 쓰기를 차단합니다.</summary>
    public void SetFencingToken(long fencingToken)
    {
        if (fencingToken <= 0)
            throw new ArgumentOutOfRangeException(nameof(fencingToken), "Fencing Token은 양수여야 합니다.");
        if (FencingToken == fencingToken) return;
        FencingToken = fencingToken;
        Version++;
    }

    public void SetStatus(string status, DateTimeOffset? completedAtUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(status);
        Status = status.Trim();
        CompletedAtUtc = completedAtUtc?.ToUniversalTime();
        Version++;
    }

    public void SetProgress(int processedRecords, int totalRecords, int errorCount, int warningCount,
        int detailItemCount, bool detailTruncated, string ruleCountsJson)
    {
        if (processedRecords < 0 || totalRecords < 0 || errorCount < 0 || warningCount < 0 || detailItemCount < 0)
            throw new ArgumentOutOfRangeException(nameof(processedRecords));
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleCountsJson);
        ProcessedRecords = processedRecords;
        TotalRecords = totalRecords;
        ErrorCount = errorCount;
        WarningCount = warningCount;
        DetailItemCount = detailItemCount;
        DetailTruncated = detailTruncated;
        RuleCountsJson = ruleCountsJson;
        Version++;
    }

    public void SetFailure(string code, string message, DateTimeOffset completedAtUtc)
    {
        FailureCode = ValidateText(code, 200, nameof(code));
        FailureMessage = ValidateText(message, 500, nameof(message));
        SetStatus("Failed", completedAtUtc);
    }

    private static string ValidateText(string value, int maxLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maxLength)
            throw new ArgumentException($"값은 {maxLength}자를 초과할 수 없습니다.", parameterName);
        return normalized;
    }
}

/// <summary>검증 결과에서 원문 값을 제외한 오류·경고 항목을 저장합니다.</summary>
public sealed class DataSetValidationDetail
{
    private DataSetValidationDetail() { }

    public DataSetValidationDetail(Guid executionId, Guid tenantId, string detailReference, int rowNumber,
        string? fieldName, string code, string severity, string message)
    {
        if (executionId == Guid.Empty) throw new ArgumentException("실행 식별자는 비어 있을 수 없습니다.", nameof(executionId));
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        if (rowNumber <= 0) throw new ArgumentOutOfRangeException(nameof(rowNumber));
        DetailReference = ValidateText(detailReference, 128, nameof(detailReference));
        FieldName = fieldName?.Trim();
        if (FieldName?.Length > 128) throw new ArgumentException("필드명은 128자를 초과할 수 없습니다.", nameof(fieldName));
        Code = ValidateText(code, 200, nameof(code));
        Severity = ValidateText(severity, 16, nameof(severity));
        Message = ValidateText(message, 500, nameof(message));
        ExecutionId = executionId;
        TenantId = tenantId;
        RowNumber = rowNumber;
    }

    public long Id { get; private set; }
    public Guid ExecutionId { get; private set; }
    public Guid TenantId { get; private set; }
    public string DetailReference { get; private set; } = string.Empty;
    public int RowNumber { get; private set; }
    public string? FieldName { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Severity { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;

    private static string ValidateText(string value, int maxLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maxLength)
            throw new ArgumentException($"값은 {maxLength}자를 초과할 수 없습니다.", parameterName);
        return normalized;
    }
}

/// <summary>중복 규칙에 사용한 원문을 저장하지 않고 해시만 보관합니다.</summary>
public sealed class DataSetValidationSeenKey
{
    private DataSetValidationSeenKey() { }

    public DataSetValidationSeenKey(Guid executionId, string ruleCode, string keyHash)
    {
        if (executionId == Guid.Empty) throw new ArgumentException("실행 식별자는 비어 있을 수 없습니다.", nameof(executionId));
        RuleCode = ValidateText(ruleCode, 200, nameof(ruleCode));
        KeyHash = ValidateText(keyHash, 64, nameof(keyHash));
        ExecutionId = executionId;
    }

    public long Id { get; private set; }
    public Guid ExecutionId { get; private set; }
    public string RuleCode { get; private set; } = string.Empty;
    public string KeyHash { get; private set; } = string.Empty;

    private static string ValidateText(string value, int maxLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maxLength)
            throw new ArgumentException($"값은 {maxLength}자를 초과할 수 없습니다.", parameterName);
        return normalized;
    }
}
