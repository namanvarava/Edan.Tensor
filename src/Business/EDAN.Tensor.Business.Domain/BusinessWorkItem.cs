namespace EDAN.Tensor.Business.Domain;
public sealed record BusinessWorkItem(Guid Id, Guid TenantId, string WorkType, string ReferenceNo, string Status, DateTimeOffset CreatedAtUtc);
