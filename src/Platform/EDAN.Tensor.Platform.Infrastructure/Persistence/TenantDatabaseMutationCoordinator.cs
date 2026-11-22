using System.Text.Json;
using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence;

public sealed class TenantDatabaseMutationCoordinator(ITenantDatabaseRoutineExecutor executor, PlatformDbContext db, TimeProvider clock) : ITenantDatabaseMutationCoordinator
{
    public async Task<TenantDatabaseMutationResult> ExecuteAsync(TenantId ownerTenantId, string idempotencyKey, IReadOnlyList<TenantDatabaseMutationStep> steps, CancellationToken cancellationToken = default)
    {
        if (steps.Count == 0) throw new ArgumentException("최소 하나의 변경 단계가 필요합니다.", nameof(steps));
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentException("멱등성 키가 필요합니다.", nameof(idempotencyKey));
        var existing = await db.TenantDatabaseMutationAudits.AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == ownerTenantId && x.IdempotencyKey == idempotencyKey, cancellationToken);
        if (existing?.ResultJson is not null)
            return JsonSerializer.Deserialize<TenantDatabaseMutationResult>(existing.ResultJson) ?? throw new InvalidOperationException("변경 감사 결과를 읽을 수 없습니다.");
        var audit = TenantDatabaseMutationAudit.Start(new TenantDatabaseMutationAuditId(Guid.NewGuid()), ownerTenantId, idempotencyKey, clock.GetUtcNow());
        db.TenantDatabaseMutationAudits.Add(audit);
        await db.SaveChangesAsync(cancellationToken);
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
                var failed = new TenantDatabaseMutationResult(false, compensated, results);
                audit.Complete(false, compensated, JsonSerializer.Serialize(failed), results.LastOrDefault(x => !x.Succeeded)?.Error, clock.GetUtcNow());
                await db.SaveChangesAsync(cancellationToken);
                return failed;
            }
        }
        var succeeded = new TenantDatabaseMutationResult(true, true, results);
        audit.Complete(true, true, JsonSerializer.Serialize(succeeded), null, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return succeeded;
    }
}
