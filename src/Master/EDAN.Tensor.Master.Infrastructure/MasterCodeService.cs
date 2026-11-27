using System.Collections.Concurrent;
using EDAN.Tensor.Master.Application;
using EDAN.Tensor.Master.Contracts;
namespace EDAN.Tensor.Master.Infrastructure;
public sealed class InMemoryMasterCodeService : IMasterCodeService
{
    private readonly ConcurrentDictionary<Guid, MasterCodeContract> _items = new();
    public IReadOnlyList<MasterCodeContract> List(Guid tenantId, string? groupCode, string? search, bool? isEnabled, int page, int pageSize, string? sort)
    {
        page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 200);
        var query = _items.Values.Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(groupCode)) query = query.Where(x => x.GroupCode.Equals(groupCode, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Code.Contains(search, StringComparison.OrdinalIgnoreCase) || x.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (isEnabled.HasValue) query = query.Where(x => x.IsEnabled == isEnabled.Value);
        query = string.Equals(sort, "name", StringComparison.OrdinalIgnoreCase) ? query.OrderBy(x => x.Name) : query.OrderBy(x => x.SortOrder).ThenBy(x => x.Code);
        return query.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
    }
    public MasterCodeContract Create(Guid tenantId, UpsertMasterCodeRequest request)
    { var item = new MasterCodeContract(Guid.NewGuid(), tenantId, request.GroupCode.Trim(), request.Code.Trim(), request.Name.Trim(), request.SortOrder, request.IsEnabled); _items[item.Id] = item; return item; }
}
