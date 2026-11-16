using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Domain.Model;
using DataSetAggregate = EDAN.Tensor.DataSet.Domain.Model.DataSet;
using EDAN.Tensor.SharedKernel.Exceptions;
using EDAN.Tensor.SharedKernel.Results;

namespace EDAN.Tensor.DataSet.Application.UseCases;

/// <summary>소유 범위 검증, 저장소 할당, DataSet 메타데이터 저장을 조정합니다.</summary>
public sealed class DataSetManagementService(
    IDataSetRepository repository,
    IDataSetScopeValidator scopeValidator,
    IDataSetStorageAdapterResolver adapters,
    IDataSetStorageQuotaPolicyProvider quotaPolicies)
{
    /// <summary>검증된 소유 범위에 DataSet 초안을 생성합니다. 메타데이터 저장 실패 시 저장소 할당을 보상 해제합니다.</summary>
    public async Task<Result<DataSetId>> CreateAsync(
        DataSetScope scope,
        string name,
        string adapterId,
        string contentFormat = "json-array",
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await scopeValidator.IsValidAsync(scope, cancellationToken))
                return Failure<DataSetId>("dataset.scope_invalid", "Tenant·Organization·Project 소유 관계가 유효하지 않습니다.");

            ArgumentException.ThrowIfNullOrWhiteSpace(adapterId);
            var adapterKey = adapterId.Trim().ToLowerInvariant();
            var adapter = adapters.Resolve(adapterKey);
            if (adapter is null)
                return Failure<DataSetId>("dataset.adapter_unavailable", "요청한 저장소 어댑터를 사용할 수 없습니다.");

            var id = new DataSetId(Guid.NewGuid());
            var reference = await adapter.AllocateAsync(new DataSetStorageAllocation(id, scope), cancellationToken);
            try
            {
                var dataSet = DataSetAggregate.Create(id, scope, name, reference, adapter.AdapterId, contentFormat);
                repository.Add(dataSet);
                await repository.SaveChangesAsync(cancellationToken);
                return Result<DataSetId>.Success(id);
            }
            catch
            {
                await adapter.ReleaseAsync(new DataSetStorageAllocation(id, scope), reference, CancellationToken.None);
                throw;
            }
        }
        catch (DomainException ex) { return Failure<DataSetId>(ex.Code, ex.Message); }
        catch (ArgumentException ex) { return Failure<DataSetId>("dataset.validation", ex.Message); }
        catch (InvalidOperationException ex) { return Failure<DataSetId>("dataset.operation_failed", ex.Message); }
    }

    /// <summary>Tenant 소유 범위에서 DataSet 콘텐츠를 기록합니다.</summary>
    public async Task<Result> WriteContentAsync(
        DataSetId id, Guid tenantId, Stream content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        var dataSet = await repository.GetAsync(id, tenantId, cancellationToken);
        if (dataSet is null)
            return Result.Failure(new Error("dataset.not_found", "Tenant 범위에서 DataSet을 찾을 수 없습니다."));
        if (dataSet.Status == DataSetStatus.Archived)
            return Result.Failure(new Error("dataset.archived", "보관된 DataSet에는 콘텐츠를 기록할 수 없습니다."));

        var adapter = adapters.Resolve(dataSet.StorageAdapter);
        if (adapter is null)
            return Result.Failure(new Error("dataset.adapter_unavailable", "저장소 어댑터를 사용할 수 없습니다."));

        try
        {
            var allocation = new DataSetStorageAllocation(dataSet.Id, dataSet.Scope);
            var policy = await quotaPolicies.GetPolicyAsync(allocation, cancellationToken);
            await adapter.WriteAsync(allocation, dataSet.StorageReference, content, policy, cancellationToken);
            return Result.Success();
        }
        catch (InvalidDataException ex)
        {
            return Result.Failure(new Error("dataset.content_too_large", ex.Message));
        }
        catch (DataSetStorageQuotaExceededException ex)
        {
            return Result.Failure(new Error("dataset.quota_exceeded", ex.Message));
        }
    }

    /// <summary>Tenant 소유 범위에서 DataSet 콘텐츠 스트림을 엽니다.</summary>
    public async Task<Stream?> OpenContentReadAsync(
        DataSetId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var dataSet = await repository.GetAsync(id, tenantId, cancellationToken);
        if (dataSet is null)
            return null;
        var adapter = adapters.Resolve(dataSet.StorageAdapter)
            ?? throw new InvalidOperationException("DataSet 저장소 어댑터를 사용할 수 없습니다.");
        return await adapter.OpenReadAsync(new DataSetStorageAllocation(dataSet.Id, dataSet.Scope),
            dataSet.StorageReference, cancellationToken);
    }
    /// <summary>Tenant 범위에서 DataSet 메타데이터를 조회합니다.</summary>
    public Task<DataSetAggregate?> GetAsync(DataSetId id, Guid tenantId, CancellationToken cancellationToken = default) =>
        repository.GetAsync(id, tenantId, cancellationToken);

    /// <summary>Tenant 범위 DataSet을 조직·프로젝트·이름·상태로 필터링해 페이지 조회합니다.</summary>
    public Task<DataSetPage> GetPageAsync(Guid tenantId, Guid? organizationId, Guid? projectId,
        string? search, DataSetStatus? status, int page, int pageSize,
        CancellationToken cancellationToken = default) =>
        repository.GetPageAsync(tenantId, organizationId, projectId, search, status, page, pageSize, cancellationToken);

    /// <summary>DataSet 및 동일 Project 소유 범위의 실제 콘텐츠 저장량을 조회합니다.</summary>
    public async Task<DataSetStorageUsage?> GetStorageUsageAsync(
        DataSetId id, Guid tenantId, IDataSetStorageUsageReader usageReader,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(usageReader);
        var dataSet = await repository.GetAsync(id, tenantId, cancellationToken);
        return dataSet is null
            ? null
            : await usageReader.GetUsageAsync(new DataSetStorageAllocation(dataSet.Id, dataSet.Scope), cancellationToken);
    }

    /// <summary>Tenant 범위가 일치하는 DataSet에 새 스키마 버전을 추가합니다.</summary>
    public async Task<Result<int>> AddSchemaAsync(
        DataSetId id, Guid tenantId, string schemaJson, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var dataSet = await repository.GetAsync(id, tenantId, cancellationToken);
        if (dataSet is null) return Failure<int>("dataset.not_found", "Tenant 범위에서 DataSet을 찾을 수 없습니다.");
        try
        {
            var revision = dataSet.AddSchema(schemaJson, nowUtc);
            await repository.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(revision.Version);
        }
        catch (DomainException ex) { return Failure<int>(ex.Code, ex.Message); }
        catch (ArgumentException ex) { return Failure<int>("dataset.validation", ex.Message); }
    }

    /// <summary>Tenant 범위가 일치하고 스키마가 등록된 DataSet을 활성화합니다.</summary>
    public async Task<Result> ActivateAsync(DataSetId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var dataSet = await repository.GetAsync(id, tenantId, cancellationToken);
        if (dataSet is null) return Result.Failure(new Error("dataset.not_found", "Tenant 범위에서 DataSet을 찾을 수 없습니다."));
        try
        {
            dataSet.Activate();
            await repository.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException ex) { return Result.Failure(new Error(ex.Code, ex.Message)); }
    }

    public Task<Result> RenameAsync(DataSetId id, Guid tenantId, string name,
        CancellationToken cancellationToken = default) => MutateAsync(id, tenantId,
        dataSet => dataSet.Rename(name), cancellationToken);

    public Task<Result> ArchiveAsync(DataSetId id, Guid tenantId,
        CancellationToken cancellationToken = default) => MutateAsync(id, tenantId,
        dataSet => dataSet.Archive(), cancellationToken);

    public Task<Result> RestoreAsync(DataSetId id, Guid tenantId,
        CancellationToken cancellationToken = default) => MutateAsync(id, tenantId,
        dataSet => dataSet.Restore(), cancellationToken);

    public async Task<Result> DeleteAsync(DataSetId id, Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var dataSet = await repository.GetAsync(id, tenantId, cancellationToken);
        if (dataSet is null)
            return Result.Failure(new Error("dataset.not_found", "Tenant 범위에서 DataSet을 찾을 수 없습니다."));
        var adapter = adapters.Resolve(dataSet.StorageAdapter);
        if (adapter is null)
            return Result.Failure(new Error("dataset.adapter_unavailable", "DataSet 저장소 어댑터를 사용할 수 없습니다."));
        try
        {
            await adapter.ReleaseAsync(new DataSetStorageAllocation(dataSet.Id, dataSet.Scope),
                dataSet.StorageReference, cancellationToken);
            repository.Remove(dataSet);
            await repository.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("dataset.delete_failed", ex.Message));
        }
    }

    /// <summary>물리 저장소를 새 참조로 교체하여 콘텐츠를 초기화합니다.</summary>
    public async Task<Result> ResetStorageAsync(DataSetId id, Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var dataSet = await repository.GetAsync(id, tenantId, cancellationToken);
        if (dataSet is null)
            return Result.Failure(new Error("dataset.not_found", "Tenant 범위에서 DataSet을 찾을 수 없습니다."));
        var adapter = adapters.Resolve(dataSet.StorageAdapter);
        if (adapter is null)
            return Result.Failure(new Error("dataset.adapter_unavailable", "DataSet 저장소 어댑터를 사용할 수 없습니다."));
        var allocation = new DataSetStorageAllocation(dataSet.Id, dataSet.Scope);
        StorageReference? replacement = null;
        try
        {
            replacement = await adapter.AllocateAsync(allocation, cancellationToken);
            var previous = dataSet.StorageReference;
            dataSet.ChangeStorageReference(replacement);
            await repository.SaveChangesAsync(cancellationToken);
            await adapter.ReleaseAsync(allocation, previous, cancellationToken);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            if (replacement is not null) await adapter.ReleaseAsync(allocation, replacement, CancellationToken.None);
            return Result.Failure(new Error(ex.Code, ex.Message));
        }
        catch (ArgumentException ex)
        {
            if (replacement is not null) await adapter.ReleaseAsync(allocation, replacement, CancellationToken.None);
            return Result.Failure(new Error("dataset.validation", ex.Message));
        }
    }

    public async Task<Result<int>> RollbackSchemaAsync(DataSetId id, Guid tenantId, int version,
        DateTimeOffset nowUtc, CancellationToken cancellationToken = default)
    {
        var dataSet = await repository.GetAsync(id, tenantId, cancellationToken);
        if (dataSet is null) return Failure<int>("dataset.not_found", "Tenant 범위에서 DataSet을 찾을 수 없습니다.");
        try
        {
            var revision = dataSet.RollbackSchema(version, nowUtc);
            await repository.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(revision.Version);
        }
        catch (DomainException ex) { return Failure<int>(ex.Code, ex.Message); }
        catch (ArgumentException ex) { return Failure<int>("dataset.validation", ex.Message); }
    }

    public async Task<Result<DataSetSchemaDiff>> GetSchemaDiffAsync(DataSetId id, Guid tenantId,
        int fromVersion, int toVersion, CancellationToken cancellationToken = default)
    {
        var dataSet = await repository.GetAsync(id, tenantId, cancellationToken);
        if (dataSet is null) return Result<DataSetSchemaDiff>.Failure(new Error("dataset.not_found", "Tenant 범위에서 DataSet을 찾을 수 없습니다."));
        var from = dataSet.GetSchema(fromVersion);
        var to = dataSet.GetSchema(toVersion);
        if (from is null || to is null)
            return Result<DataSetSchemaDiff>.Failure(new Error("dataset.schema_not_found", "요청한 스키마 revision을 찾을 수 없습니다."));
        return Result<DataSetSchemaDiff>.Success(new DataSetSchemaDiff(from.Version, to.Version,
            from.SchemaJson, to.SchemaJson, !string.Equals(from.SchemaJson, to.SchemaJson, StringComparison.Ordinal)));
    }

    private async Task<Result> MutateAsync(DataSetId id, Guid tenantId, Action<DataSetAggregate> mutation,
        CancellationToken cancellationToken)
    {
        var dataSet = await repository.GetAsync(id, tenantId, cancellationToken);
        if (dataSet is null)
            return Result.Failure(new Error("dataset.not_found", "Tenant 범위에서 DataSet을 찾을 수 없습니다."));
        try
        {
            mutation(dataSet);
            await repository.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException ex) { return Result.Failure(new Error(ex.Code, ex.Message)); }
        catch (ArgumentException ex) { return Result.Failure(new Error("dataset.validation", ex.Message)); }
    }

    private static Result<T> Failure<T>(string code, string message) =>
        Result<T>.Failure(new Error(code, message));
}

public sealed record DataSetSchemaDiff(int FromVersion, int ToVersion, string FromSchemaJson,
    string ToSchemaJson, bool Changed);
