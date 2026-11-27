namespace EDAN.Tensor.System.Domain;

public sealed record SystemMenu(Guid Id, Guid TenantId, string Code, string Name, string Path, bool IsEnabled);
