using System.Reflection;
using EDAN.Tensor.Worker.Application.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EDAN.Tensor.Worker.Infrastructure.Tests;

/// <summary>실제 Dispatcher 소스를 대상으로 커서 수명과 LRU 메모리 상한을 검증합니다.</summary>
public sealed class WorkerTaskDispatcherCursorTests
{
    [Fact]
    public void CursorOverflowEvictsLeastRecentlyUsedTenantAndKeepsRecentlyReadTenant()
    {
        using var dispatcher = CreateDispatcher(500);
        var tenants = Enumerable.Range(0, 500).Select(_ => Guid.NewGuid()).ToArray();
        var taskId = Guid.NewGuid();
        foreach (var tenant in tenants) Store(dispatcher, tenant, taskId);
        Assert.Equal(taskId, Read(dispatcher, tenants[0]));
        var added = Guid.NewGuid();
        Store(dispatcher, added, taskId);

        Assert.Null(Read(dispatcher, tenants[1]));
        Assert.Equal(taskId, Read(dispatcher, tenants[0]));
        Assert.Equal(taskId, Read(dispatcher, added));
        Assert.Equal(500, CursorCount(dispatcher));
    }

    [Fact]
    public void UpdatingAndRemovingCursorDoesNotLeaveStaleLruEntries()
    {
        using var dispatcher = CreateDispatcher(500);
        var tenant = Guid.NewGuid();
        Store(dispatcher, tenant, Guid.NewGuid());
        var latest = Guid.NewGuid();
        Store(dispatcher, tenant, latest);
        Assert.Equal(latest, Read(dispatcher, tenant));
        Assert.Equal(1, CursorCount(dispatcher));
        Invoke(dispatcher, "RemoveTenantTaskCursor", tenant);
        Invoke(dispatcher, "RemoveTenantTaskCursor", tenant);
        Assert.Null(Read(dispatcher, tenant));
        Assert.Equal(0, CursorCount(dispatcher));
        Assert.Equal(0, LruCount(dispatcher));
    }

    [Fact]
    public void ConfiguredLimitBelowMinimumIsClampedToFiveHundred()
    {
        using var dispatcher = CreateDispatcher(1);
        for (var i = 0; i < 501; i++) Store(dispatcher, Guid.NewGuid(), Guid.NewGuid());
        Assert.Equal(500, CursorCount(dispatcher));
        Assert.Equal(500, LruCount(dispatcher));
    }

    /// <summary>테스트는 설정이 없을 때 Tenant 작업 커서가 기본 상한 10,000을 사용하는지 검증합니다.</summary>
    [Fact]
    public void MissingLimitUsesTenThousandDefault()
    {
        using var dispatcher = CreateDispatcher(null);
        for (var i = 0; i < 10_001; i++) Store(dispatcher, Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(10_000, CursorCount(dispatcher));
        Assert.Equal(10_000, LruCount(dispatcher));
    }

    /// <summary>테스트는 설정된 Tenant 작업 커서 상한이 최대값 100,000으로 제한되는지 검증합니다.</summary>
    [Fact]
    public void ConfiguredLimitAboveMaximumIsClampedToOneHundredThousand()
    {
        using var dispatcher = CreateDispatcher(100_001);
        for (var i = 0; i < 100_001; i++) Store(dispatcher, Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(100_000, CursorCount(dispatcher));
        Assert.Equal(100_000, LruCount(dispatcher));
    }

    private static WorkerTaskDispatcher CreateDispatcher(int? limit)
    {
        var values = new Dictionary<string, string?>();
        if (limit.HasValue)
            values["Worker:TenantTaskCursorLimit"] = limit.Value.ToString();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        // 커서 연산은 DB나 Handler를 사용하지 않습니다.
        return new WorkerTaskDispatcher(NullLogger<WorkerTaskDispatcher>.Instance,
            null!, new WorkerExecutionRegistry(), null!, configuration, TimeProvider.System);
    }

    private static void Store(WorkerTaskDispatcher dispatcher, Guid tenant, Guid task) =>
        Invoke(dispatcher, "StoreTenantTaskCursor", tenant, task);

    private static Guid? Read(WorkerTaskDispatcher dispatcher, Guid tenant)
    {
        object?[] arguments = [tenant, Guid.Empty];
        var found = (bool)typeof(WorkerTaskDispatcher).GetMethod("TryGetTenantTaskCursor",
            BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(dispatcher, arguments)!;
        return found ? (Guid)arguments[1]! : null;
    }

    private static void Invoke(WorkerTaskDispatcher dispatcher, string method, params object?[] arguments) =>
        typeof(WorkerTaskDispatcher).GetMethod(method,
            BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(dispatcher, arguments);

    private static int CursorCount(WorkerTaskDispatcher dispatcher) => Count(dispatcher, "_tenantTaskCursors");
    private static int LruCount(WorkerTaskDispatcher dispatcher) => Count(dispatcher, "_tenantTaskCursorLru");
    private static int Count(WorkerTaskDispatcher dispatcher, string field)
    {
        var collection = typeof(WorkerTaskDispatcher).GetField(field,
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(dispatcher)!;
        return (int)collection.GetType().GetProperty("Count")!.GetValue(collection)!;
    }
}
