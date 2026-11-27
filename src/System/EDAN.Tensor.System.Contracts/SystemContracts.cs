namespace EDAN.Tensor.System.Contracts;

public sealed record SystemMenuContract(Guid Id, Guid TenantId, string Code, string Name, string Path, bool IsEnabled);
public sealed record UpsertSystemMenuRequest(string Code, string Name, string Path, bool IsEnabled = true);
public sealed record SystemMenuPageContract(IReadOnlyList<SystemMenuContract> Items, int TotalCount, int Page, int PageSize);
public sealed record SystemApiErrorContract(string Code, string Description);
