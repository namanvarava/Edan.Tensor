using System.Text.Json;
using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Application.Validation;
using EDAN.Tensor.DataSet.Domain.Model;
using EDAN.Tensor.SharedKernel.Results;

namespace EDAN.Tensor.DataSet.Application.UseCases;

/// <summary>Tenant 범위의 검증 프로필과 비동기 DataSet 검증 실행을 조정합니다.</summary>
public sealed class DataSetValidationService(
    IDataSetRepository dataSets,
    IDataSetValidationRepository validations,
    DataSetManagementService dataSetManagement)
{
    /// <summary>프로필 문서를 검증한 뒤 첫 번째 불변 revision을 생성합니다.</summary>
    public async Task<Result<DataSetValidationProfileSnapshot>> CreateProfileAsync(
        Guid tenantId, string name, string definitionJson, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        try
        {
            EnsureTenant(tenantId);
            DataSetValidationProfileParser.Parse(definitionJson);
            return Result<DataSetValidationProfileSnapshot>.Success(await validations.CreateProfileAsync(
                tenantId, name, definitionJson, nowUtc, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return Failure<DataSetValidationProfileSnapshot>("dataset.validation.profile_invalid", exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Failure<DataSetValidationProfileSnapshot>("dataset.validation.profile_conflict", exception.Message);
        }
    }

    /// <summary>새 규칙 문서를 기존 프로필의 다음 불변 revision으로 추가합니다.</summary>
    public async Task<Result<DataSetValidationProfileSnapshot>> AddProfileRevisionAsync(
        Guid tenantId, Guid profileId, string definitionJson, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        try
        {
            EnsureTenant(tenantId);
            DataSetValidationProfileParser.Parse(definitionJson);
            return Result<DataSetValidationProfileSnapshot>.Success(await validations.AddProfileRevisionAsync(
                tenantId, profileId, definitionJson, nowUtc, cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return Failure<DataSetValidationProfileSnapshot>("dataset.validation.profile_not_found", exception.Message);
        }
        catch (ArgumentException exception)
        {
            return Failure<DataSetValidationProfileSnapshot>("dataset.validation.profile_invalid", exception.Message);
        }
    }

    /// <summary>현재 DataSet·스키마·프로필 revision을 고정한 검증 실행을 예약합니다.</summary>
    public async Task<Result<DataSetValidationExecutionSnapshot>> CreateExecutionAsync(
        Guid tenantId, Guid dataSetId, DataSetValidationRequest request, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        try
        {
            EnsureTenant(tenantId);
            if (request.ExpectedDataSetVersion <= 0 || request.SchemaVersion <= 0
                || request.ProfileId == Guid.Empty || request.ProfileRevision <= 0)
                return Failure<DataSetValidationExecutionSnapshot>(
                    "dataset.validation.request_invalid", "검증 실행 요청의 버전과 식별자가 올바르지 않습니다.");

            var dataSet = await dataSets.GetAsync(new DataSetId(dataSetId), tenantId, cancellationToken);
            if (dataSet is null)
                return Failure<DataSetValidationExecutionSnapshot>("dataset.not_found", "Tenant 범위에서 DataSet을 찾을 수 없습니다.");
            if (dataSet.Status != DataSetStatus.Active)
                return Failure<DataSetValidationExecutionSnapshot>("dataset.validation.dataset_inactive", "활성 상태의 DataSet만 검증할 수 있습니다.");
            if (dataSet.Version != request.ExpectedDataSetVersion)
                return Failure<DataSetValidationExecutionSnapshot>("dataset.validation.version_mismatch", "검증 대상 DataSet 버전이 현재 버전과 다릅니다.");
            if (!dataSet.SchemaRevisions.Any(x => x.Version == request.SchemaVersion))
                return Failure<DataSetValidationExecutionSnapshot>("dataset.validation.schema_not_found", "검증 대상 스키마 revision을 찾을 수 없습니다.");
            if (await validations.GetProfileAsync(tenantId, request.ProfileId, request.ProfileRevision, cancellationToken) is null)
                return Failure<DataSetValidationExecutionSnapshot>("dataset.validation.profile_not_found", "Tenant 범위에서 검증 프로필 revision을 찾을 수 없습니다.");

            return Result<DataSetValidationExecutionSnapshot>.Success(await validations.CreateExecutionAsync(
                tenantId, dataSetId, request.ExpectedDataSetVersion, request.SchemaVersion,
                request.ProfileId, request.ProfileRevision, nowUtc, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return Failure<DataSetValidationExecutionSnapshot>("dataset.validation.request_invalid", exception.Message);
        }
    }

    /// <summary>Worker가 실행 참조를 처리하고 진행 상태를 영속화합니다.</summary>
    /// <param name="fencingToken">현재 Worker Lease가 발급한 양수 Fencing Token을 지정합니다.</param>
    public async Task<DataSetValidationRunOutcome> RunAsync(
        Guid tenantId, Guid executionId, long fencingToken,
        CancellationToken cancellationToken = default)
    {
        if (fencingToken <= 0)
            throw new ArgumentOutOfRangeException(nameof(fencingToken), "Fencing Token은 양수여야 합니다.");

        var execution = await validations.GetExecutionAsync(tenantId, executionId, cancellationToken)
            ?? throw new KeyNotFoundException("Tenant 범위에서 검증 실행을 찾을 수 없습니다.");
        if (execution.Status is DataSetValidationStatus.Valid or DataSetValidationStatus.Invalid
            or DataSetValidationStatus.Failed)
            return DataSetValidationRunOutcome.AlreadyCompleted;

        try
        {
            if (!await validations.TryStartAsync(tenantId, executionId, fencingToken, cancellationToken))
                return execution.Status is DataSetValidationStatus.Valid
                    or DataSetValidationStatus.Invalid
                    or DataSetValidationStatus.Failed
                    ? DataSetValidationRunOutcome.AlreadyCompleted
                    : DataSetValidationRunOutcome.QueuedForRetry;

            var dataSet = await dataSets.GetAsync(new DataSetId(execution.DataSetId), tenantId, cancellationToken);
            var profileSnapshot = await validations.GetProfileAsync(tenantId, execution.ProfileId,
                execution.ProfileRevision, cancellationToken);
            if (dataSet is null || profileSnapshot is null
                || dataSet.Version != execution.ExpectedDataSetVersion
                || dataSet.Status != DataSetStatus.Active
                || !dataSet.SchemaRevisions.Any(x => x.Version == execution.SchemaVersion))
            {
                var failed = await validations.FailAsync(tenantId, executionId, fencingToken, "dataset.validation.input_changed",
                    "검증 예약 이후 DataSet, 스키마 또는 프로필 revision이 더 이상 일치하지 않습니다.", CancellationToken.None);
                return failed ? DataSetValidationRunOutcome.Completed : DataSetValidationRunOutcome.QueuedForRetry;
            }

            var profile = DataSetValidationProfileParser.Parse(profileSnapshot.DefinitionJson);
            await using var content = await dataSetManagement.OpenContentReadAsync(
                new DataSetId(execution.DataSetId), tenantId, cancellationToken);
            if (content is null)
            {
                var failed = await validations.FailAsync(tenantId, executionId, fencingToken, "dataset.validation.content_not_found",
                    "검증할 DataSet 콘텐츠를 찾을 수 없습니다.", CancellationToken.None);
                return failed ? DataSetValidationRunOutcome.Completed : DataSetValidationRunOutcome.QueuedForRetry;
            }

            var records = await DataSetValidationContentReader.ReadAsync(content, dataSet.ContentFormat,
                profile.MaxRecords, profile.MaxFieldLength, cancellationToken);
            var seenKeys = (await validations.GetSeenKeysAsync(executionId, cancellationToken)).ToHashSet(StringComparer.Ordinal);
            var detailCount = execution.DetailItemCount;
            var detailTruncated = execution.DetailTruncated;
            for (var index = 0; index < records.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var row = records[index];
                var result = DataSetValidationEngine.ValidateRow(row, profile, seenKeys);
                var remaining = Math.Max(0, profile.MaxDetailItems - detailCount);
                var details = result.Findings.Take(remaining).ToArray();
                detailCount += details.Length;
                detailTruncated |= details.Length < result.Findings.Count;
                if (!await validations.RecordRowAsync(tenantId, executionId, fencingToken, row.RowNumber,
                    result.Findings, details, result.DuplicateObservations, index + 1, records.Count,
                    detailTruncated, cancellationToken))
                    return DataSetValidationRunOutcome.QueuedForRetry;
            }

            var final = await validations.GetExecutionAsync(tenantId, executionId, cancellationToken)
                ?? throw new KeyNotFoundException("검증 실행이 사라졌습니다.");
            var completed = await validations.CompleteAsync(tenantId, executionId, fencingToken,
                final.ErrorCount == 0 ? DataSetValidationStatus.Valid : DataSetValidationStatus.Invalid,
                records.Count, detailTruncated, final.RuleCountsJson, cancellationToken);
            return completed ? DataSetValidationRunOutcome.Completed : DataSetValidationRunOutcome.QueuedForRetry;
        }
        catch (DataSetValidationInputException exception)
        {
            var failed = await validations.FailAsync(tenantId, executionId, fencingToken,
                exception.Code, exception.Message, CancellationToken.None);
            return failed ? DataSetValidationRunOutcome.Completed : DataSetValidationRunOutcome.QueuedForRetry;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await validations.MarkQueuedAsync(tenantId, executionId, fencingToken, CancellationToken.None);
            return DataSetValidationRunOutcome.QueuedForRetry;
        }
        catch
        {
            await validations.MarkQueuedAsync(tenantId, executionId, fencingToken, CancellationToken.None);
            throw;
        }
    }

    /// <summary>Tenant 범위의 검증 실행 요약을 조회합니다.</summary>
    public Task<DataSetValidationExecutionSnapshot?> GetExecutionAsync(
        Guid tenantId, Guid executionId, CancellationToken cancellationToken = default) =>
        validations.GetExecutionAsync(tenantId, executionId, cancellationToken);

    /// <summary>Tenant 범위에서 원문 값 없이 저장된 검증 상세를 페이지 단위로 조회합니다.</summary>
    public Task<IReadOnlyList<DataSetValidationDetailSnapshot>> GetDetailsAsync(
        Guid tenantId, Guid executionId, int skip, int take, CancellationToken cancellationToken = default) =>
        validations.GetDetailsAsync(tenantId, executionId, skip, take, cancellationToken);

    /// <summary>종료된 검증 결과와 상세를 Tenant 관리자의 요청으로 삭제합니다.</summary>
    public Task<bool> DeleteExecutionAsync(Guid tenantId, Guid executionId,
        CancellationToken cancellationToken = default) =>
        validations.DeleteExecutionAsync(tenantId, executionId, cancellationToken);

    private static void EnsureTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
    }

    private static Result<T> Failure<T>(string code, string description) =>
        Result<T>.Failure(new Error(code, description));
}
