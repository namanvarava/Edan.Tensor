using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence;

/// <summary>외부 전달 상태를 Worker SQLite에 Tenant 범위로 저장합니다.</summary>
public sealed class ExternalDeliveryRepository(WorkerDbContext dbContext) : IExternalDeliveryRepository
{
    /// <inheritdoc />
    public async Task<ExternalDeliveryCreateResult> CreateOrGetAsync(Guid tenantId, string targetUri, string payload,
        string idempotencyKey, int maxAttempts, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var delivery = ExternalDelivery.Create(tenantId, targetUri, payload, idempotencyKey, maxAttempts, nowUtc);
        dbContext.ExternalDeliveries.Add(delivery);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return new ExternalDeliveryCreateResult(ToSnapshot(delivery), Created: true);
        }
        catch (DbUpdateException)
        {
            // 동시에 같은 멱등 키를 저장한 요청이 있으면 새 엔티티를 분리하고 커밋된 기존 값을 다시 읽습니다.
            dbContext.Entry(delivery).State = EntityState.Detached;
            var existing = await GetByIdempotencyKeyAsync(tenantId, idempotencyKey, cancellationToken);
            if (existing is null)
                throw;
            return new ExternalDeliveryCreateResult(existing, Created: false);
        }
        catch
        {
            dbContext.Entry(delivery).State = EntityState.Detached;
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ExternalDeliverySnapshot> CreateAsync(Guid tenantId, string targetUri, string payload,
        string idempotencyKey, int maxAttempts, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        return (await CreateOrGetAsync(tenantId, targetUri, payload, idempotencyKey, maxAttempts,
            nowUtc, cancellationToken)).Delivery;
    }

    /// <inheritdoc />
    public async Task<ExternalDeliverySnapshot?> GetAsync(Guid tenantId, Guid deliveryId,
        CancellationToken cancellationToken = default)
    {
        var delivery = await dbContext.ExternalDeliveries.AsNoTracking()
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == deliveryId, cancellationToken);
        return delivery is null ? null : ToSnapshot(delivery);
    }

    /// <inheritdoc />
    public async Task<ExternalDeliverySnapshot?> GetByIdempotencyKeyAsync(Guid tenantId, string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var delivery = await dbContext.ExternalDeliveries.AsNoTracking()
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.IdempotencyKey == idempotencyKey,
                cancellationToken);
        return delivery is null ? null : ToSnapshot(delivery);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteUnqueuedAsync(Guid tenantId, Guid deliveryId, string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var delivery = await dbContext.ExternalDeliveries.SingleOrDefaultAsync(
            x => x.TenantId == tenantId
                && x.Id == deliveryId
                && x.IdempotencyKey == idempotencyKey
                && x.Status == ExternalDeliveryStatus.Pending
                && x.AttemptCount == 0
                && x.CompletedAtUtc == null,
            cancellationToken);
        if (delivery is null)
            return false;

        dbContext.ExternalDeliveries.Remove(delivery);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <inheritdoc />
    public async Task<ExternalDeliverySnapshot?> BeginAttemptAsync(Guid tenantId, Guid deliveryId,
        CancellationToken cancellationToken = default)
    {
        var delivery = await dbContext.ExternalDeliveries
            .SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == deliveryId, cancellationToken);
        if (delivery is null) return null;
        delivery.BeginAttempt();
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToSnapshot(delivery);
    }

    /// <inheritdoc />
    public async Task CompleteAsync(Guid tenantId, Guid deliveryId, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var delivery = await GetTrackedAsync(tenantId, deliveryId, cancellationToken);
        delivery.MarkSucceeded(nowUtc);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task MarkTransientFailureAsync(Guid tenantId, Guid deliveryId, string errorCode,
        CancellationToken cancellationToken = default)
    {
        var delivery = await GetTrackedAsync(tenantId, deliveryId, cancellationToken);
        delivery.MarkTransientFailure(errorCode);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task MarkPermanentFailureAsync(Guid tenantId, Guid deliveryId, string errorCode,
        DateTimeOffset nowUtc, CancellationToken cancellationToken = default)
    {
        var delivery = await GetTrackedAsync(tenantId, deliveryId, cancellationToken);
        delivery.MarkPermanentFailure(errorCode, nowUtc);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<ExternalDelivery> GetTrackedAsync(Guid tenantId, Guid deliveryId,
        CancellationToken cancellationToken) =>
        await dbContext.ExternalDeliveries.SingleOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == deliveryId, cancellationToken)
        ?? throw new KeyNotFoundException("Tenant 범위에서 외부 전달을 찾을 수 없습니다.");

    private static ExternalDeliverySnapshot ToSnapshot(ExternalDelivery delivery) => new(
        delivery.Id, delivery.TenantId, delivery.TargetUri, delivery.Payload, delivery.IdempotencyKey,
        delivery.Status, delivery.AttemptCount, delivery.MaxAttempts, delivery.LastErrorCode,
        delivery.CreatedAtUtc, delivery.CompletedAtUtc);
}
