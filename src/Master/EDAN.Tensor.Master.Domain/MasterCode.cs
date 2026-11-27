namespace EDAN.Tensor.Master.Domain;
public sealed record MasterCode(Guid Id, Guid TenantId, string GroupCode, string Code, string Name, int SortOrder, bool IsEnabled);
