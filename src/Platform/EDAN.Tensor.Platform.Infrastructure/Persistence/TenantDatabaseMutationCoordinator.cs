using EDAN.Tensor.Platform.Application.Abstractions;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence;

public sealed class TenantDatabaseMutationCoordinator(ITenantDatabaseRoutineExecutor executor) : ITenantDatabaseMutationCoordinator
{
    public async Task<TenantDatabaseMutationResult> ExecuteAsync(IReadOnlyList<TenantDatabaseMutationStep> steps, CancellationToken cancellationToken = default)
    {
        if (steps.Count == 0) throw new ArgumentException("최소 하나의 변경 단계가 필요합니다.", nameof(steps));
        var results = new List<TenantDatabaseMutationStepResult>();
        var completed = new List<(int Index, TenantDatabaseMutationStep Step)>();
        for (var i = 0; i < steps.Count; i++)
        {
            try
            {
                await executor.ExecuteAsync(steps[i].TenantId, steps[i].Kind, steps[i].SchemaName, steps[i].RoutineName, steps[i].Parameters, cancellationToken);
                results.Add(new(i, true, false, null)); completed.Add((i, steps[i]));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                results.Add(new(i, false, false, ex.Message));
                var compensated = true;
                foreach (var item in completed.AsEnumerable().Reverse())
                {
                    if (item.Step.Compensation is null) continue;
                    try
                    {
                        var c = item.Step.Compensation;
                        await executor.ExecuteAsync(c.TenantId, c.Kind, c.SchemaName, c.RoutineName, c.Parameters, cancellationToken);
                        results.Add(new(item.Index, true, true, null));
                    }
                    catch (Exception compensationError) when (compensationError is not OperationCanceledException)
                    {
                        compensated = false; results.Add(new(item.Index, false, true, compensationError.Message));
                    }
                }
                return new(false, compensated, results);
            }
        }
        return new(true, true, results);
    }
}
