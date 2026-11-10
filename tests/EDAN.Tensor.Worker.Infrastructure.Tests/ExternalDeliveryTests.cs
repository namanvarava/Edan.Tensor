using System.Net;
using System.Net.Sockets;
using System.Text;
using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Application.Execution;
using EDAN.Tensor.Worker.Application.ExternalDelivery;
using EDAN.Tensor.Worker.Domain.Model;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using EDAN.Tensor.WorkerHost.Handlers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EDAN.Tensor.Worker.Infrastructure.Tests;

public sealed class ExternalDeliveryTests
{
    /// <summary>Development가 아닌 환경에서 loopback 허용 옵션을 거부합니다.</summary>
    [Fact]
    public void LoopbackAllowanceIsRejectedOutsideDevelopment()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ExternalDeliveryOptions.Create(
                new HashSet<string>(StringComparer.OrdinalIgnoreCase), true,
                TimeSpan.FromSeconds(1), isDevelopment: false));

        Assert.Contains("Development", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>Development 환경에서는 격리된 loopback 테스트 수신기를 허용합니다.</summary>
    [Fact]
    public void LoopbackAllowanceIsAcceptedInDevelopment()
    {
        var options = ExternalDeliveryOptions.Create(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase), true,
            TimeSpan.FromSeconds(1), isDevelopment: true);

        Assert.True(options.AllowLoopback);
    }

    /// <summary>조정기가 이미 취소된 작업의 외부 전달 실행을 시작하지 않습니다.</summary>
    [Fact]
    public async Task HandlerRejectsCancellationBeforeStartingDelivery()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var handler = new ExternalDeliveryTaskHandler(provider.GetRequiredService<IServiceScopeFactory>());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var task = WorkerTask.Create(Guid.NewGuid(), ExternalDeliveryTaskContract.TaskType,
            "cancelled-delivery", ExternalDeliveryTaskContract.CreatePayloadReference(Guid.NewGuid()),
            DateTimeOffset.UtcNow);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => handler.StartAsync(task, cancellation.Token));
    }

    /// <summary>조정기의 실행 취소가 진행 중인 외부 HTTP 호출까지 전파됩니다.</summary>
    [Fact]
    public async Task HandlerPropagatesCancellationToInFlightDelivery()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var dbOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(dbOptions);
        await db.Database.MigrateAsync();
        var tenantId = Guid.NewGuid();
        var delivery = await new ExternalDeliveryRepository(db).CreateAsync(
            tenantId, "http://localhost:4567/receiver", "{\"event\":\"cancel\"}",
            "delivery-cancel", 3, DateTimeOffset.UtcNow);
        var receiver = new CancellationObservingHandler();
        using var client = new HttpClient(receiver) { Timeout = TimeSpan.FromSeconds(30) };

        var services = new ServiceCollection();
        services.AddScoped<IExternalDeliveryRepository>(_ => new ExternalDeliveryRepository(db));
        services.AddSingleton(client);
        services.AddSingleton(new ExternalDeliveryOptions(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase), true, TimeSpan.FromSeconds(30)));
        services.AddScoped<ExternalDeliveryService>();
        using var provider = services.BuildServiceProvider();
        var handler = new ExternalDeliveryTaskHandler(provider.GetRequiredService<IServiceScopeFactory>());
        var task = WorkerTask.Create(tenantId, ExternalDeliveryTaskContract.TaskType,
            "cancelled-delivery-in-flight", ExternalDeliveryTaskContract.CreatePayloadReference(delivery.Id),
            DateTimeOffset.UtcNow);
        using var cancellation = new CancellationTokenSource();

        await using var execution = await handler.StartAsync(task, cancellation.Token);
        await receiver.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();

        await receiver.Cancelled.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var result = await execution.Completion;
        Assert.False(result.Succeeded);
        Assert.Equal("external.delivery.execution_failed", result.ErrorCode);
    }

    [Fact]
    public async Task CreateOrGet_ReturnsExistingDeliveryWithoutCreatingDuplicate()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();
        var repository = new ExternalDeliveryRepository(db);
        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var first = await repository.CreateOrGetAsync(tenantId, "https://receiver.test/events",
            "{\"event\":\"created\"}", "delivery-create-once", 3, now);
        var second = await repository.CreateOrGetAsync(tenantId, "https://receiver.test/events",
            "{\"event\":\"created\"}", "delivery-create-once", 3, now.AddMinutes(1));

        Assert.True(first.Created);
        Assert.False(second.Created);
        Assert.Equal(first.Delivery.Id, second.Delivery.Id);
        Assert.Equal(1, await db.ExternalDeliveries.CountAsync(x =>
            x.TenantId == tenantId && x.IdempotencyKey == "delivery-create-once"));
    }

    [Fact]
    public async Task ResponseLoss_ReusesSameIdempotencyKeyAndSucceedsOnRetry()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();
        var repository = new ExternalDeliveryRepository(db);
        var handler = new RecordingHandler(attempt =>
            attempt == 1 ? throw new HttpRequestException("응답 유실") : new HttpResponseMessage(HttpStatusCode.OK));
        using var client = new HttpClient(handler);
        var service = new ExternalDeliveryService(repository, client,
            new ExternalDeliveryOptions(new HashSet<string>(StringComparer.OrdinalIgnoreCase), true,
                TimeSpan.FromSeconds(1)));
        var tenantId = Guid.NewGuid();
        var created = await service.CreateAsync(tenantId, "http://127.0.0.1:4567/receiver",
            "{\"event\":\"created\"}", "delivery-1", 3, DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<ExternalDeliveryTransientException>(() =>
            service.DeliverAsync(tenantId, created.Id));
        var pending = await service.GetAsync(tenantId, created.Id);
        Assert.NotNull(pending);
        Assert.Equal("Pending", pending!.Status.ToString());
        Assert.Equal("external.delivery.response_lost", pending.LastErrorCode);

        var succeeded = await service.DeliverAsync(tenantId, created.Id);
        Assert.Equal("Succeeded", succeeded.Status.ToString());
        Assert.Equal(2, succeeded.AttemptCount);
        Assert.Equal(2, handler.Keys.Count);
        Assert.Single(handler.Keys.Distinct(StringComparer.Ordinal));
    }

    [Fact]
    public async Task LocalReceiver_ResponseLossThenSuccess_ReusesSameIdempotencyKey()
    {
        await using var receiver = new LocalReceiver();
        using var receiverTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var receiverTask = receiver.RunAsync(receiverTimeout.Token);
        using var client = new HttpClient(new SocketsHttpHandler { UseProxy = false })
        {
            Timeout = TimeSpan.FromSeconds(2)
        };
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var dbOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(dbOptions);
        await db.Database.MigrateAsync();
        var service = new ExternalDeliveryService(
            new ExternalDeliveryRepository(db), client,
            new ExternalDeliveryOptions(new HashSet<string>(StringComparer.OrdinalIgnoreCase), true,
                TimeSpan.FromSeconds(2)));
        var tenantId = Guid.NewGuid();
        var created = await service.CreateAsync(tenantId, receiver.TargetUri,
            "{\"event\":\"local-receiver\"}", "local-receiver-1", 3, DateTimeOffset.UtcNow);

        var firstAttempt = service.DeliverAsync(tenantId, created.Id);
        var firstCompleted = await Task.WhenAny(firstAttempt, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.Same(firstAttempt, firstCompleted);
        await Assert.ThrowsAsync<ExternalDeliveryTransientException>(() => firstAttempt);
        var succeeded = await service.DeliverAsync(tenantId, created.Id);
        await receiverTask;

        Assert.Equal("Succeeded", succeeded.Status.ToString());
        Assert.Equal(2, succeeded.AttemptCount);
        Assert.Equal(2, receiver.IdempotencyKeys.Count);
        Assert.Single(receiver.IdempotencyKeys.Distinct(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Http5xx_IsRetryableAndTimeoutIsRecordedWithoutRawResponse()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();
        var repository = new ExternalDeliveryRepository(db);
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        using var client = new HttpClient(handler);
        var service = new ExternalDeliveryService(repository, client,
            new ExternalDeliveryOptions(new HashSet<string>(StringComparer.OrdinalIgnoreCase), true,
                TimeSpan.FromSeconds(1)));
        var tenantId = Guid.NewGuid();
        var created = await service.CreateAsync(tenantId, "http://localhost:4567/receiver",
            "{\"secret\":\"not-logged\"}", "delivery-5xx", 3, DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<ExternalDeliveryTransientException>(() =>
            service.DeliverAsync(tenantId, created.Id));
        var failed = await service.GetAsync(tenantId, created.Id);
        Assert.Equal("Pending", failed!.Status.ToString());
        Assert.Equal("external.delivery.http_5xx", failed.LastErrorCode);
        Assert.DoesNotContain("not-logged", failed.LastErrorCode ?? string.Empty, StringComparison.Ordinal);

        using var timeoutClient = new HttpClient(new TimeoutHandler())
        {
            Timeout = TimeSpan.FromMilliseconds(20)
        };
        var timeoutService = new ExternalDeliveryService(repository, timeoutClient,
            new ExternalDeliveryOptions(new HashSet<string>(StringComparer.OrdinalIgnoreCase), true,
                TimeSpan.FromMilliseconds(20)));
        var timeoutDelivery = await timeoutService.CreateAsync(tenantId, "http://localhost:4567/receiver",
            "{\"timeout\":true}", "delivery-timeout", 3, DateTimeOffset.UtcNow);
        await Assert.ThrowsAsync<ExternalDeliveryTransientException>(() =>
            timeoutService.DeliverAsync(tenantId, timeoutDelivery.Id));
        var timedOut = await timeoutService.GetAsync(tenantId, timeoutDelivery.Id);
        Assert.Equal("external.delivery.timeout", timedOut!.LastErrorCode);
    }

    [Fact]
    public async Task DeliveryState_IsTenantScoped()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();
        var repository = new ExternalDeliveryRepository(db);
        var delivery = await repository.CreateAsync(Guid.NewGuid(), "https://receiver.test/events",
            "{\"ok\":true}", "delivery-tenant", 3, DateTimeOffset.UtcNow);

        Assert.Null(await repository.GetAsync(Guid.NewGuid(), delivery.Id));
        Assert.NotNull(await repository.GetAsync(delivery.TenantId, delivery.Id));
    }

    private sealed class RecordingHandler(
        Func<int, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        private int _attempt;
        public List<string> Keys { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Keys.Add(request.Headers.GetValues("Idempotency-Key").Single());
            var response = responseFactory(Interlocked.Increment(ref _attempt));
            return Task.FromResult(response);
        }
    }

    private sealed class TimeoutHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }

    private sealed class CancellationObservingHandler : HttpMessageHandler
    {
        public TaskCompletionSource Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Cancelled { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Started.TrySetResult();
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (OperationCanceledException)
            {
                Cancelled.TrySetResult();
                throw;
            }
        }
    }

    private sealed class LocalReceiver : IAsyncDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);

        public LocalReceiver()
        {
            _listener.Start();
            TargetUri = $"http://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}/events";
        }

        public string TargetUri { get; }
        public List<string> IdempotencyKeys { get; } = [];

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            for (var attempt = 1; attempt <= 2; attempt++)
            {
                using var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                await using var stream = client.GetStream();
                var request = await ReadHeadersAsync(stream, cancellationToken);
                var key = request.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
                    .First(line => line.StartsWith("Idempotency-Key:", StringComparison.OrdinalIgnoreCase))
                    .Split(':', 2)[1].Trim();
                IdempotencyKeys.Add(key);
                if (attempt == 2)
                {
                    var response = Encoding.ASCII.GetBytes(
                        "HTTP/1.1 200 OK\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
                    await stream.WriteAsync(response);
                }
            }
        }

        public ValueTask DisposeAsync()
        {
            _listener.Stop();
            return ValueTask.CompletedTask;
        }

        private static async Task<string> ReadHeadersAsync(NetworkStream stream,
            CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];
            var request = new StringBuilder();
            while (!request.ToString().Contains("\r\n\r\n", StringComparison.Ordinal))
            {
                var count = await stream.ReadAsync(buffer, cancellationToken);
                if (count == 0)
                    break;
                request.Append(Encoding.ASCII.GetString(buffer, 0, count));
                if (request.Length > 32_768)
                    throw new InvalidOperationException("로컬 수신기 요청 헤더가 너무 큽니다.");
            }

            return request.ToString();
        }
    }
}
