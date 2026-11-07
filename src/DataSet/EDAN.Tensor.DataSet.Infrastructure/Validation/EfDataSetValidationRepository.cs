using System.Text.Json;
using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Application.Validation;
using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.DataSet.Infrastructure.Validation;

/// <summary>검증 프로필·실행·상세 결과를 DataSet 전용 DbContext에 저장합니다.</summary>
public sealed class EfDataSetValidationRepository(DataSetDbContext dbContext) : IDataSetValidationRepository
{
    public async Task<DataSetValidationProfileSnapshot?> GetProfileAsync(
        Guid tenantId, Guid profileId, int revision, CancellationToken cancellationToken = default)
    {
        var profileRevision = await dbContext.ValidationProfileRevisions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.TenantId == tenantId
                && x.ProfileId == profileId && x.Revision == revision, cancellationToken);
        if (profileRevision is null) return null;
        var profile = await dbContext.ValidationProfiles.AsNoTracking()
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == profileId, cancellationToken);
        return profile is null
            ? null
            : new DataSetValidationProfileSnapshot(profile.Id, profile.TenantId, profile.Name,
                profileRevision.Revision, profileRevision.DefinitionJson, profileRevision.CreatedAtUtc);
    }

    public async Task<DataSetValidationProfileSnapshot> CreateProfileAsync(
        Guid tenantId, string name, string definitionJson, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var profile = new DataSetValidationProfile(Guid.NewGuid(), tenantId, name, nowUtc);
        var revision = new DataSetValidationProfileRevision(
            profile.Id, tenantId, 1, definitionJson, nowUtc);
        dbContext.ValidationProfiles.Add(profile);
        dbContext.ValidationProfileRevisions.Add(revision);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new DataSetValidationProfileSnapshot(profile.Id, tenantId, profile.Name,
            revision.Revision, revision.DefinitionJson, revision.CreatedAtUtc);
    }

    public async Task<DataSetValidationProfileSnapshot> AddProfileRevisionAsync(
        Guid tenantId, Guid profileId, string definitionJson, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.ValidationProfiles
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == profileId, cancellationToken)
            ?? throw new KeyNotFoundException("Tenant 범위에서 검증 프로필을 찾을 수 없습니다.");
        var revision = await dbContext.ValidationProfileRevisions
            .Where(x => x.TenantId == tenantId && x.ProfileId == profileId)
            .MaxAsync(x => (int?)x.Revision, cancellationToken) ?? 0;
        var next = new DataSetValidationProfileRevision(profile.Id, tenantId, revision + 1,
            definitionJson, nowUtc);
        dbContext.ValidationProfileRevisions.Add(next);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new DataSetValidationProfileSnapshot(profile.Id, tenantId, profile.Name,
            next.Revision, next.DefinitionJson, next.CreatedAtUtc);
    }

    public async Task<DataSetValidationExecutionSnapshot> CreateExecutionAsync(
        Guid tenantId, Guid dataSetId, int expectedDataSetVersion, int schemaVersion,
        Guid profileId, int profileRevision, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var execution = new DataSetValidationExecution(Guid.NewGuid(), tenantId, dataSetId,
            expectedDataSetVersion, schemaVersion, profileId, profileRevision,
            "validation-detail:" + Guid.NewGuid().ToString("N"), nowUtc);
        dbContext.ValidationExecutions.Add(execution);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToSnapshot(execution);
    }

    public async Task<DataSetValidationExecutionSnapshot?> GetExecutionAsync(
        Guid tenantId, Guid executionId, CancellationToken cancellationToken = default)
    {
        var execution = await dbContext.ValidationExecutions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == executionId, cancellationToken);
        return execution is null ? null : ToSnapshot(execution);
    }

    public async Task<bool> TryStartAsync(
        Guid tenantId, Guid executionId, long fencingToken,
        CancellationToken cancellationToken = default)
    {
        if (fencingToken <= 0)
            throw new ArgumentOutOfRangeException(nameof(fencingToken));
        var execution = await dbContext.ValidationExecutions
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == executionId, cancellationToken);
        if (execution is null) return false;
        if (execution.Status is nameof(DataSetValidationStatus.Valid)
            or nameof(DataSetValidationStatus.Invalid)
            or nameof(DataSetValidationStatus.Failed))
            return false;
        if (execution.Status == nameof(DataSetValidationStatus.Running))
            return execution.FencingToken == fencingToken;
        if (execution.Status != nameof(DataSetValidationStatus.Queued)) return false;
        execution.SetFencingToken(fencingToken);
        execution.SetStatus(nameof(DataSetValidationStatus.Running));
        return await SaveFencedAsync(execution, cancellationToken);
    }

    public async Task<IReadOnlySet<string>> GetSeenKeysAsync(
        Guid executionId, CancellationToken cancellationToken = default)
    {
        var keys = await dbContext.ValidationSeenKeys.AsNoTracking()
            .Where(x => x.ExecutionId == executionId)
            .Select(x => x.RuleCode + "\u001f" + x.KeyHash)
            .ToArrayAsync(cancellationToken);
        return keys.ToHashSet(StringComparer.Ordinal);
    }

    public async Task<bool> RecordRowAsync(
        Guid tenantId, Guid executionId, long fencingToken, int rowNumber,
        IReadOnlyList<DataSetValidationFinding> findings,
        IReadOnlyList<DataSetValidationFinding> detailFindings,
        IReadOnlyList<DataSetValidationDuplicateObservation> duplicateObservations,
        int processedRecords, int totalRecords, bool detailTruncated,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(findings);
        ArgumentNullException.ThrowIfNull(detailFindings);
        ArgumentNullException.ThrowIfNull(duplicateObservations);
        if (fencingToken <= 0)
            throw new ArgumentOutOfRangeException(nameof(fencingToken));
        if (rowNumber <= 0 || processedRecords < rowNumber)
            throw new ArgumentOutOfRangeException(nameof(rowNumber));

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var execution = await dbContext.ValidationExecutions
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == executionId
                && x.FencingToken == fencingToken, cancellationToken);
        if (execution is null) return false;
        if (execution.ProcessedRecords >= rowNumber)
        {
            await transaction.RollbackAsync(cancellationToken);
            return true;
        }
        if (execution.Status != nameof(DataSetValidationStatus.Running))
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        foreach (var observation in duplicateObservations)
        {
            var exists = await dbContext.ValidationSeenKeys.AnyAsync(
                x => x.ExecutionId == executionId
                    && x.RuleCode == observation.RuleCode
                    && x.KeyHash == observation.KeyHash, cancellationToken);
            if (!exists)
                dbContext.ValidationSeenKeys.Add(new DataSetValidationSeenKey(
                    executionId, observation.RuleCode, observation.KeyHash));
        }

        var ruleCounts = JsonSerializer.Deserialize<Dictionary<string, int>>(execution.RuleCountsJson)
            ?? new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var finding in findings)
        {
            ruleCounts.TryGetValue(finding.Code, out var count);
            ruleCounts[finding.Code] = count + 1;
        }

        foreach (var finding in detailFindings)
            dbContext.ValidationDetails.Add(new DataSetValidationDetail(
                executionId, tenantId, execution.DetailReference, rowNumber,
                finding.FieldName, finding.Code, finding.Severity.ToString(), finding.Message));

        var errors = findings.Count(x => x.Severity == DataSetValidationSeverity.Error);
        var warnings = findings.Count(x => x.Severity == DataSetValidationSeverity.Warning);
        execution.SetProgress(processedRecords, totalRecords,
            execution.ErrorCount + errors, execution.WarningCount + warnings,
            execution.DetailItemCount + detailFindings.Count,
            execution.DetailTruncated || detailTruncated,
            JsonSerializer.Serialize(ruleCounts));
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task<bool> CompleteAsync(
        Guid tenantId, Guid executionId, long fencingToken, DataSetValidationStatus status,
        int totalRecords, bool detailTruncated, string ruleCountsJson,
        CancellationToken cancellationToken = default)
    {
        if (status is not (DataSetValidationStatus.Valid or DataSetValidationStatus.Invalid))
            throw new ArgumentOutOfRangeException(nameof(status));
        if (fencingToken <= 0)
            throw new ArgumentOutOfRangeException(nameof(fencingToken));
        var execution = await dbContext.ValidationExecutions
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == executionId
                && x.FencingToken == fencingToken, cancellationToken);
        if (execution is null) return false;
        execution.SetProgress(execution.ProcessedRecords, totalRecords,
            execution.ErrorCount, execution.WarningCount, execution.DetailItemCount,
            execution.DetailTruncated || detailTruncated, ruleCountsJson);
        execution.SetStatus(status.ToString(), DateTimeOffset.UtcNow);
        return await SaveFencedAsync(execution, cancellationToken);
    }

    public async Task<bool> MarkQueuedAsync(
        Guid tenantId, Guid executionId, long fencingToken,
        CancellationToken cancellationToken = default)
    {
        if (fencingToken <= 0)
            throw new ArgumentOutOfRangeException(nameof(fencingToken));
        var execution = await dbContext.ValidationExecutions
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == executionId
                && x.FencingToken == fencingToken, cancellationToken);
        if (execution is null) return false;
        if (execution.Status == nameof(DataSetValidationStatus.Running))
        {
            execution.SetStatus(nameof(DataSetValidationStatus.Queued));
            return await SaveFencedAsync(execution, cancellationToken);
        }
        return execution.Status == nameof(DataSetValidationStatus.Queued);
    }

    public async Task<bool> FailAsync(
        Guid tenantId, Guid executionId, long fencingToken, string code, string message,
        CancellationToken cancellationToken = default)
    {
        if (fencingToken <= 0)
            throw new ArgumentOutOfRangeException(nameof(fencingToken));
        var execution = await dbContext.ValidationExecutions
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == executionId
                && x.FencingToken == fencingToken, cancellationToken);
        if (execution is null) return false;
        if (execution.Status is nameof(DataSetValidationStatus.Valid)
            or nameof(DataSetValidationStatus.Invalid)
            or nameof(DataSetValidationStatus.Failed))
            return true;
        execution.SetFailure(code, message, DateTimeOffset.UtcNow);
        return await SaveFencedAsync(execution, cancellationToken);
    }

    public async Task<IReadOnlyList<DataSetValidationDetailSnapshot>> GetDetailsAsync(
        Guid tenantId, Guid executionId, int skip, int take,
        CancellationToken cancellationToken = default)
    {
        if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));
        if (take is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(take));
        var details = await dbContext.ValidationDetails.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ExecutionId == executionId)
            .OrderBy(x => x.RowNumber).ThenBy(x => x.Id)
            .Skip(skip).Take(take)
            .ToArrayAsync(cancellationToken);
        return details.Select(x => new DataSetValidationDetailSnapshot(
            x.RowNumber, x.FieldName, x.Code,
            Enum.Parse<DataSetValidationSeverity>(x.Severity), x.Message)).ToArray();
    }

    public async Task<bool> DeleteExecutionAsync(
        Guid tenantId, Guid executionId, CancellationToken cancellationToken = default)
    {
        var execution = await dbContext.ValidationExecutions
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == executionId, cancellationToken);
        if (execution is null) return false;
        if (execution.Status is nameof(DataSetValidationStatus.Queued)
            or nameof(DataSetValidationStatus.Running))
            throw new InvalidOperationException("진행 중인 검증 결과는 삭제할 수 없습니다.");
        dbContext.ValidationDetails.RemoveRange(dbContext.ValidationDetails.Where(x => x.ExecutionId == executionId));
        dbContext.ValidationSeenKeys.RemoveRange(dbContext.ValidationSeenKeys.Where(x => x.ExecutionId == executionId));
        dbContext.ValidationExecutions.Remove(execution);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static DataSetValidationExecutionSnapshot ToSnapshot(DataSetValidationExecution execution) =>
        new(execution.Id, execution.TenantId, execution.DataSetId,
            execution.ExpectedDataSetVersion, execution.SchemaVersion, execution.ProfileId,
            execution.ProfileRevision,
            execution.FencingToken,
            Enum.Parse<DataSetValidationStatus>(execution.Status),
            execution.DetailReference, execution.ProcessedRecords, execution.TotalRecords,
            execution.ErrorCount, execution.WarningCount, execution.DetailItemCount,
            execution.DetailTruncated, execution.RuleCountsJson, execution.FailureCode,
            execution.FailureMessage, execution.CreatedAtUtc, execution.CompletedAtUtc);

    private async Task<bool> SaveFencedAsync(
        DataSetValidationExecution execution, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.ChangeTracker.Clear();
            return false;
        }
    }
}
