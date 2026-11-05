using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.ApiHost.Services;

/// <summary>실제 콘텐츠 DB의 Tenant 사용량을 시간 단위로 멱등 기록하고 조회합니다.</summary>
public sealed class DataSetStorageUsageSnapshotService(
    DataSetDbContext dataSetDb,
    IDataSetTenantStorageUsageReader usageReader,
    TimeProvider timeProvider)
{
    public async Task<int> CaptureDueAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var period = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, 0, 0, TimeSpan.Zero);
        var tenantIds = await dataSetDb.DataSets.AsNoTracking()
            .Select(x => x.TenantId).Distinct().OrderBy(x => x).ToListAsync(cancellationToken);

        foreach (var tenantId in tenantIds)
        {
            var usage = await usageReader.GetTenantUsageAsync(tenantId, cancellationToken);
            var snapshot = await dataSetDb.StorageUsageSnapshots.SingleOrDefaultAsync(
                x => x.TenantId == tenantId && x.PeriodStartUtc == period, cancellationToken);
            if (snapshot is null)
            {
                dataSetDb.StorageUsageSnapshots.Add(new DataSetStorageUsageSnapshot
                {
                    TenantId = tenantId, PeriodStartUtc = period, CapturedAtUtc = now,
                    UsedBytes = usage.TenantBytes, DataSetCount = usage.TenantDataSetCount
                });
            }
            else
            {
                snapshot.CapturedAtUtc = now;
                snapshot.UsedBytes = usage.TenantBytes;
                snapshot.DataSetCount = usage.TenantDataSetCount;
            }
        }
        await dataSetDb.SaveChangesAsync(cancellationToken);
        return tenantIds.Count;
    }

    public Task<List<DataSetStorageUsageSnapshot>> GetHistoryAsync(
        Guid tenantId, int take = 168, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        return dataSetDb.StorageUsageSnapshots.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.PeriodStartUtc)
            .Take(Math.Clamp(take, 1, 744))
            .ToListAsync(cancellationToken);
    }
}
