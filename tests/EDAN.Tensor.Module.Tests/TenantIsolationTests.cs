using EDAN.Tensor.Business.Contracts;
using EDAN.Tensor.Business.Infrastructure;
using EDAN.Tensor.Master.Contracts;
using EDAN.Tensor.Master.Infrastructure;
using EDAN.Tensor.System.Contracts;
using EDAN.Tensor.System.Infrastructure;
using Xunit;

namespace EDAN.Tensor.Module.Tests;

public sealed class TenantIsolationTests
{
    [Fact]
    public void SystemMenusAreScopedToTenant()
    {
        var service = new InMemorySystemMenuService();
        var first = Guid.NewGuid(); var second = Guid.NewGuid();
        service.Create(first, new UpsertSystemMenuRequest("HOME", "Home", "/home"));
        Assert.Empty(service.List(second, null, null, 1, 50, null));
        Assert.Single(service.List(first, null, null, 1, 50, null));
    }

    [Fact]
    public void MasterCodesAreScopedToTenant()
    {
        var service = new InMemoryMasterCodeService();
        var first = Guid.NewGuid(); var second = Guid.NewGuid();
        service.Create(first, new UpsertMasterCodeRequest("STATUS", "READY", "Ready"));
        Assert.Empty(service.List(second, null, null, null, 1, 50, null));
        Assert.Single(service.List(first, null, null, null, 1, 50, null));
    }

    [Fact]
    public void BusinessWorkItemsAreScopedToTenant()
    {
        var service = new InMemoryBusinessWorkItemService();
        var first = Guid.NewGuid(); var second = Guid.NewGuid();
        service.Create(first, new CreateBusinessWorkItemRequest("ORDER", "ORD-001"));
        Assert.Empty(service.List(second, null, null, 1, 50, null));
        Assert.Single(service.List(first, null, null, 1, 50, null));
    }
}
