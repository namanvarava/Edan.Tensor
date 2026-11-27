using EDAN.Tensor.Business.Contracts;
namespace EDAN.Tensor.Business.Application;
public interface IBusinessWorkItemService
{
    IReadOnlyList<BusinessWorkItemContract> List(Guid tenantId, string? workType, string? status, int page, int pageSize, string? sort);
    BusinessWorkItemContract Create(Guid tenantId, CreateBusinessWorkItemRequest request);
}
