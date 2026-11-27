namespace EDAN.Tensor.Business.Contracts;
public sealed record BusinessWorkItemContract(Guid Id, Guid TenantId, string WorkType, string ReferenceNo, string Status, DateTimeOffset CreatedAtUtc);
public sealed record CreateBusinessWorkItemRequest(string WorkType, string ReferenceNo);
public sealed record BusinessWorkItemPageContract(IReadOnlyList<BusinessWorkItemContract> Items, int TotalCount, int Page, int PageSize);
