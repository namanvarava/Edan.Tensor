using EDAN.Tensor.Master.Contracts;
namespace EDAN.Tensor.Master.Application;
public interface IMasterCodeService
{
    IReadOnlyList<MasterCodeContract> List(Guid tenantId, string? groupCode, string? search, bool? isEnabled, int page, int pageSize, string? sort);
    MasterCodeContract Create(Guid tenantId, UpsertMasterCodeRequest request);
}
