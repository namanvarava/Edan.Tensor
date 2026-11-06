using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Domain.Model;

namespace EDAN.Tensor.WorkerHost.Services;

/// <summary>Worker의 DataSet 읽기 조합점에서 저장소가 Tenant 경계를 검사하도록 명시합니다.</summary>
public sealed class WorkerDataSetScopeValidator : IDataSetScopeValidator
{
    /// <summary>Worker는 생성 API를 제공하지 않으며 실제 조회는 Tenant ID 포함 저장소가 제한합니다.</summary>
    public Task<bool> IsValidAsync(DataSetScope scope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(true);
    }
}
