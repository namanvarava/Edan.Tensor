using System.Net;
using System.Text;
using EDAN.Tensor.Client;
using Xunit;

namespace EDAN.Tensor.Client.Tests;

public sealed class TensorApiClientV1Tests
{
    [Fact]
    public async Task GetSystemMenusV1Async_UsesVersionedTenantRoute()
    {
        var tenantId = Guid.NewGuid();
        var client = new TensorApiClient(new HttpClient(new Handler(request =>
        {
            Assert.Equal("/api/v1/system/menus", request.RequestUri!.AbsolutePath);
            Assert.Contains("page=2", request.RequestUri.Query, StringComparison.Ordinal);
            Assert.Equal(tenantId.ToString("D"), request.Headers.GetValues("X-Tenant-Id").Single());
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"items\":[],\"totalCount\":0,\"page\":2,\"pageSize\":10}", Encoding.UTF8, "application/json")
            };
        })) { BaseAddress = new Uri("https://api.example.test") });

        var result = await client.GetSystemMenusV1Async(tenantId, page: 2, pageSize: 10);

        Assert.Empty(result.Items);
        Assert.Equal(2, result.Page);
    }

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> callback) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(callback(request));
    }
}
