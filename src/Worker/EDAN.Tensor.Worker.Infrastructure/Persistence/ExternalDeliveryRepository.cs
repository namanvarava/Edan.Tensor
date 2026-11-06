using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence;

/// <summary>외부 전달 상태를 Worker SQLite에 Tenant 범위로 저장합니다.</summary>
public sealed class ExternalDeliveryRepository(WorkerDbContext dbContext) : IExternalDeliveryRepository
{
    /// <inheritdoc />
    public async Task<ExternalDeliverySnapshot> CreateAsync(Guid tenantId, string targetUri, string payload,
        string idempotencyKey, int maxAttempts, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var delivery = ExternalDelivery.Create(tenantId, targetUri, payload, idempotencyKey, maxAttempts, nowUtc);
        dbContext.ExternalDeliveries.Add(delivery);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToSnapshot(delivery);
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
