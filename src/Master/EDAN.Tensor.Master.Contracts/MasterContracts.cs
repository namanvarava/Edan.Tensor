namespace EDAN.Tensor.Master.Contracts;

public sealed record MasterCodeContract(Guid Id, Guid TenantId, string GroupCode, string Code, string Name, int SortOrder, bool IsEnabled);
public sealed record UpsertMasterCodeRequest(string GroupCode, string Code, string Name, int SortOrder = 0, bool IsEnabled = true);
public sealed record MasterCodePageContract(IReadOnlyList<MasterCodeContract> Items, int TotalCount, int Page, int PageSize);
