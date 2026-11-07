using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace EDAN.Tensor.ApiHost.IntegrationTests;

/// <summary>실제 JwtBearer 경계에서 로컬 IdP 메타데이터와 JWKS 키 회전을 검증합니다.</summary>
public sealed class PlatformJwksAuthenticationTests
{
    [Fact]
    public async Task JwtBearer_ValidatesIssuerAudienceLifetimeSignatureKidAndRotation()
    {
        await using var idp = new LocalJwksServer();
        using var factory = new JwksApiHostFactory(idp.MetadataAddress, idp.Issuer);
        using var client = factory.CreateClient();
        var endpoint = $"/api/platform/tenants/{Guid.NewGuid()}/datasets/{Guid.NewGuid()}";
        using var keyOne = TestKey.Create("key-one");
        using var keyTwo = TestKey.Create("key-two");
        idp.SetKey(keyOne);

        var firstStatus = await SendAsync(client, endpoint,
            CreateToken(keyOne, idp.Issuer, "edan-tensor-api"));
        Assert.True(firstStatus == HttpStatusCode.Forbidden,
            $"JWT status={firstStatus}; IdP requests={string.Join(",", idp.Requests)}");
        idp.SetKeys(keyOne, keyTwo);
        Assert.Contains("key-two", idp.CurrentKids);
        Assert.Equal(HttpStatusCode.Unauthorized, await SendAsync(client, endpoint,
            CreateToken(keyOne, "http://wrong-issuer", "edan-tensor-api")));
        Assert.Equal(HttpStatusCode.Unauthorized, await SendAsync(client, endpoint,
            CreateToken(keyOne, idp.Issuer, "wrong-audience")));
        Assert.Equal(HttpStatusCode.Unauthorized, await SendAsync(client, endpoint,
            CreateToken(keyOne, idp.Issuer, "edan-tensor-api", DateTimeOffset.UtcNow.AddMinutes(-5))));
        var rotated = HttpStatusCode.Unauthorized;
        for (var attempt = 0; attempt < 3 && rotated == HttpStatusCode.Unauthorized; attempt++)
            rotated = await SendAsync(client, endpoint,
                CreateToken(keyTwo, idp.Issuer, "edan-tensor-api"));
        Assert.True(rotated == HttpStatusCode.Forbidden,
            $"Rotation status={rotated}; IdP requests={string.Join(",", idp.Requests)}");

        using var invalidSignatureKey = TestKey.Create("key-one");
        Assert.Equal(HttpStatusCode.Unauthorized, await SendAsync(client, endpoint,
            CreateToken(invalidSignatureKey, idp.Issuer, "edan-tensor-api")));
        using var unknownKidKey = TestKey.Create("unknown-kid");
        Assert.Equal(HttpStatusCode.Unauthorized, await SendAsync(client, endpoint,
            CreateToken(unknownKidKey, idp.Issuer, "edan-tensor-api")));
    }

    private static async Task<HttpStatusCode> SendAsync(HttpClient client, string endpoint, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await client.SendAsync(request);
        return response.StatusCode;
    }

    private static string CreateToken(TestKey key, string issuer, string audience,
        DateTimeOffset? expiresUtc = null)
    {
        var expiration = expiresUtc ?? DateTimeOffset.UtcNow.AddMinutes(5);
        var notBefore = expiration <= DateTimeOffset.UtcNow
            ? expiration.AddMinutes(-1)
            : DateTimeOffset.UtcNow.AddMinutes(-1);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            [new Claim("sub", "jwks-test-subject")],
            notBefore.UtcDateTime,
            expiration.UtcDateTime,
            new SigningCredentials(new RsaSecurityKey(key.Rsa) { KeyId = key.Kid }, SecurityAlgorithms.RsaSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class TestKey : IDisposable
    {
        private TestKey(string kid, RSA rsa) { Kid = kid; Rsa = rsa; }
        public string Kid { get; }
        public RSA Rsa { get; }
        public static TestKey Create(string kid) => new(kid, RSA.Create(2048));
        public void Dispose() => Rsa.Dispose();
        public JsonElement ToJson() => JsonSerializer.SerializeToElement(new
        {
            kty = "RSA", use = "sig", alg = "RS256", kid = Kid,
            n = Base64UrlEncoder.Encode(Rsa.ExportParameters(false).Modulus!),
            e = Base64UrlEncoder.Encode(Rsa.ExportParameters(false).Exponent!)
        });
    }

    private sealed class LocalJwksServer : IAsyncDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource _stop = new();
        private readonly Task _loop;
        private readonly object _sync = new();
        public List<string> Requests { get; } = [];
        private JsonElement[] _keys = [];

        public LocalJwksServer()
        {
            _listener.Start();
            var port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            Issuer = $"http://127.0.0.1:{port}";
            MetadataAddress = Issuer + "/.well-known/openid-configuration";
            _loop = AcceptLoopAsync(_stop.Token);
        }

        public string Issuer { get; }
        public string MetadataAddress { get; }
        public string CurrentKid
        {
            get { lock (_sync) return _keys[0].GetProperty("kid").GetString()!; }
        }
        public IReadOnlyList<string> CurrentKids
        {
            get { lock (_sync) return _keys.Select(key => key.GetProperty("kid").GetString()!).ToArray(); }
        }

        public void SetKey(TestKey key)
        {
            SetKeys(key);
        }

        public void SetKeys(params TestKey[] keys)
        {
            lock (_sync) _keys = keys.Select(key => key.ToJson()).ToArray();
        }

        public async ValueTask DisposeAsync()
        {
            _stop.Cancel();
            _listener.Stop();
            try { await _loop; } catch (OperationCanceledException) { }
            _stop.Dispose();
        }

        private async Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                    _ = HandleAsync(client, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
            catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested) { }
        }

        private async Task HandleAsync(TcpClient client, CancellationToken cancellationToken)
        {
            using var connection = client;
            using var stream = connection.GetStream();
            var buffer = new byte[4096];
            var length = 0;
            while (length < buffer.Length && !Encoding.ASCII.GetString(buffer, 0, length).Contains("\r\n\r\n", StringComparison.Ordinal))
            {
                var read = await stream.ReadAsync(buffer.AsMemory(length), cancellationToken);
                if (read == 0) return;
                length += read;
            }
            var requestLine = Encoding.ASCII.GetString(buffer, 0, length).Split("\r\n", StringSplitOptions.None)[0];
            var path = requestLine.Split(' ', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1) ?? "/";
            lock (_sync) Requests.Add(path);
            JsonElement[] currentKeys;
            lock (_sync) currentKeys = _keys.Select(key => key.Clone()).ToArray();
            if (path == "/jwks")
                lock (_sync) Requests.Add("kid=" + string.Join("+", currentKeys.Select(key => key.GetProperty("kid").GetString())));
            var body = path == "/jwks"
                ? JsonSerializer.Serialize(new { keys = currentKeys })
                : JsonSerializer.Serialize(new
                {
                    issuer = Issuer,
                    jwks_uri = Issuer + "/jwks",
                    id_token_signing_alg_values_supported = new[] { "RS256" },
                    response_types_supported = new[] { "token" }
                });
            var bytes = Encoding.UTF8.GetBytes(body);
            var header = $"HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: {bytes.Length}\r\nConnection: close\r\n\r\n";
            await stream.WriteAsync(Encoding.ASCII.GetBytes(header), cancellationToken);
            await stream.WriteAsync(bytes, cancellationToken);
        }
    }

    private sealed class JwksApiHostFactory(string metadataAddress, string issuer) : WebApplicationFactory<Program>
    {
        private readonly string _platform = Path.Combine(Path.GetTempPath(), $"edan-jwks-platform-{Guid.NewGuid():N}.db");
        private readonly string _worker = Path.Combine(Path.GetTempPath(), $"edan-jwks-worker-{Guid.NewGuid():N}.db");
        private readonly string _dataset = Path.Combine(Path.GetTempPath(), $"edan-jwks-dataset-{Guid.NewGuid():N}.db");
        private readonly string _content = Path.Combine(Path.GetTempPath(), $"edan-jwks-content-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureHostConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PlatformDatabase"] = $"Data Source={_platform};Pooling=False",
                ["ConnectionStrings:WorkerDatabase"] = $"Data Source={_worker};Pooling=False",
                ["ConnectionStrings:DataSetDatabase"] = $"Data Source={_dataset};Pooling=False",
                ["ConnectionStrings:DataSetContentDatabase"] = $"Data Source={_content};Pooling=False",
                ["Database:ApplyMigrationsInDevelopment"] = "true",
                ["Authentication:Mode"] = "External",
                ["Authentication:Issuer"] = issuer,
                ["Authentication:Audience"] = "edan-tensor-api",
                ["Authentication:MetadataAddress"] = metadataAddress,
                ["Authentication:RequireHttpsMetadata"] = "false"
            }));
            return base.CreateHost(builder);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach (var path in new[] { _platform, _worker, _dataset, _content })
                if (File.Exists(path)) File.Delete(path);
        }
    }
}
