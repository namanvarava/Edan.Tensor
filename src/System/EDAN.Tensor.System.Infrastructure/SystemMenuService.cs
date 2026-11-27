using System.Collections.Concurrent;
using EDAN.Tensor.System.Application;
using EDAN.Tensor.System.Contracts;

namespace EDAN.Tensor.System.Infrastructure;

public sealed class InMemorySystemMenuService : ISystemMenuService
{
    private readonly ConcurrentDictionary<Guid, SystemMenuContract> _items = new();
    public IReadOnlyList<SystemMenuContract> List(Guid tenantId, string? search, bool? isEnabled, int page, int pageSize, string? sort)
    {
        page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 200);
        var query = _items.Values.Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Code.Contains(search, StringComparison.OrdinalIgnoreCase) || x.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (isEnabled.HasValue) query = query.Where(x => x.IsEnabled == isEnabled.Value);
        query = string.Equals(sort, "name", StringComparison.OrdinalIgnoreCase) ? query.OrderBy(x => x.Name) : query.OrderBy(x => x.Code);
        return query.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
    }
    public SystemMenuContract Create(Guid tenantId, UpsertSystemMenuRequest request)
    {
        var item = new SystemMenuContract(Guid.NewGuid(), tenantId, request.Code.Trim(), request.Name.Trim(), request.Path.Trim(), request.IsEnabled);
        _items[item.Id] = item; return item;
    }
}
