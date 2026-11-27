using EDAN.Tensor.System.Contracts;

namespace EDAN.Tensor.System.Application;

public interface ISystemMenuService
{
    IReadOnlyList<SystemMenuContract> List(Guid tenantId, string? search, bool? isEnabled, int page, int pageSize, string? sort);
    SystemMenuContract Create(Guid tenantId, UpsertSystemMenuRequest request);
}
