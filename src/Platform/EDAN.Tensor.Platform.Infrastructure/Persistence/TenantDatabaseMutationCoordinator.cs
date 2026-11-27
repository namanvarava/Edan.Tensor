using System.Text.Json;
using System.Data.Common;
using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence;

public sealed class TenantDatabaseMutationCoordinator(
    ITenantDatabaseRoutineExecutor executor,
    PlatformDbContext db,
    TimeProvider clock,
    IConfiguration? configuration = null) : ITenantDatabaseMutationCoordinator
{
    private const int DefaultMaxAttempts = 3;
    private const int DefaultRetryDelayMilliseconds = 250;

    public async Task<TenantDatabaseMutationResult> ExecuteAsync(TenantId ownerTenantId, string idempotencyKey, IReadOnlyList<TenantDatabaseMutationStep> steps, CancellationToken cancellationToken = default)
    {
        if (steps.Count == 0) throw new ArgumentException("최소 하나의 변경 단계가 필요합니다.", nameof(steps));
        if (steps.Count > 100) throw new ArgumentException("한 번의 변경 요청에는 최대 100개 단계만 허용됩니다.", nameof(steps));
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentException("멱등성 키가 필요합니다.", nameof(idempotencyKey));
        if (steps.Any(x => x.TenantId != ownerTenantId ||
            x.Compensation is { } compensation && compensation.TenantId != ownerTenantId))
            throw new UnauthorizedAccessException("변경 단계는 요청 소유 Tenant의 DB만 대상으로 할 수 있습니다.");
        var existing = await db.TenantDatabaseMutationAudits.AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == ownerTenantId && x.IdempotencyKey == idempotencyKey, cancellationToken);
        if (existing?.ResultJson is not null)
            return JsonSerializer.Deserialize<TenantDatabaseMutationResult>(existing.ResultJson) ?? throw new InvalidOperationException("변경 감사 결과를 읽을 수 없습니다.");
        if (existing is not null)
            throw new InvalidOperationException("동일한 멱등성 키의 변경 요청이 이미 실행 중입니다.");
        var audit = TenantDatabaseMutationAudit.Start(new TenantDatabaseMutationAuditId(Guid.NewGuid()), ownerTenantId, idempotencyKey, clock.GetUtcNow());
        db.TenantDatabaseMutationAudits.Add(audit);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var concurrent = await db.TenantDatabaseMutationAudits.AsNoTracking()
                .SingleOrDefaultAsync(x => x.TenantId == ownerTenantId && x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (concurrent?.ResultJson is not null)
                return JsonSerializer.Deserialize<TenantDatabaseMutationResult>(concurrent.ResultJson)
                    ?? throw new InvalidOperationException("동시 변경 감사 결과를 읽을 수 없습니다.");
            throw new InvalidOperationException("동일한 멱등성 키의 변경 요청이 이미 실행 중입니다.");
        }
        var results = new List<TenantDatabaseMutationStepResult>();
        var completed = new List<(int Index, TenantDatabaseMutationStep Step)>();
        for (var i = 0; i < steps.Count; i++)
        {
            try
            {
                await ExecuteWithRetryAsync(steps[i], cancellationToken);
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
                        await ExecuteWithRetryAsync(c, cancellationToken);
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
        var succeeded = new TenantDatabaseMutationResult(true, false, results);
        audit.Complete(true, false, JsonSerializer.Serialize(succeeded), null, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return succeeded;
    }

    private async Task ExecuteWithRetryAsync(TenantDatabaseMutationStep step, CancellationToken cancellationToken)
    {
        var maxAttempts = ReadInt("TenantDatabaseMutation:MaxAttempts", DefaultMaxAttempts, 1, 10);
        var delayMilliseconds = ReadInt("TenantDatabaseMutation:RetryDelayMilliseconds", DefaultRetryDelayMilliseconds, 0, 30_000);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                if (step.BindingId is { } bindingId)
                    await executor.ExecuteAsync(bindingId, step.TenantId, step.Kind, step.SchemaName, step.RoutineName, step.Parameters, cancellationToken);
                else
                    await executor.ExecuteAsync(step.TenantId, step.Kind, step.SchemaName, step.RoutineName, step.Parameters, cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts && IsTransient(ex))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(delayMilliseconds * attempt), cancellationToken);
            }
        }
    }

    private int ReadInt(string key, int fallback, int minimum, int maximum) =>
        Math.Clamp(int.TryParse(configuration?[key], out var value) ? value : fallback, minimum, maximum);

    private static bool IsTransient(Exception exception) => exception switch
    {
        TimeoutException => true,
        IOException => true,
        DbException => true,
        _ => exception.InnerException is not null && IsTransient(exception.InnerException)
    };
}
