using System.Collections.Concurrent;
using EDAN.Tensor.Business.Application;
using EDAN.Tensor.Business.Contracts;
namespace EDAN.Tensor.Business.Infrastructure;
public sealed class InMemoryBusinessWorkItemService : IBusinessWorkItemService
{
    private readonly ConcurrentDictionary<Guid, BusinessWorkItemContract> _items = new();
    public IReadOnlyList<BusinessWorkItemContract> List(Guid tenantId, string? workType, string? status, int page, int pageSize, string? sort)
    {
        page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 200);
        var query = _items.Values.Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(workType)) query = query.Where(x => x.WorkType.Equals(workType, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        query = string.Equals(sort, "referenceNo", StringComparison.OrdinalIgnoreCase) ? query.OrderBy(x => x.ReferenceNo) : query.OrderByDescending(x => x.CreatedAtUtc);
        return query.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
    }
    public BusinessWorkItemContract Create(Guid tenantId, CreateBusinessWorkItemRequest request)
    { var item = new BusinessWorkItemContract(Guid.NewGuid(), tenantId, request.WorkType.Trim(), request.ReferenceNo.Trim(), "Pending", DateTimeOffset.UtcNow); _items[item.Id] = item; return item; }
}
