using System.Reflection;
using EDAN.Tensor.Worker.Application.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EDAN.Tensor.Worker.Infrastructure.Tests;

/// <summary>
/// 테스트는 실제 Dispatcher와 Registry가 종료 인계 시간 예산을 공유하는지 검증합니다.
/// 테스트는 실행 중인 작업을 재현하기 위해 내부 활성 작업 등록 경계를 호출하며 DB와 업무 Handler는 사용하지 않습니다.
/// </summary>
public sealed class WorkerTaskDispatcherShutdownTests
{
    /// <summary>
    /// 테스트는 Registry가 인계 예산을 모두 사용한 경우 Dispatcher가 같은 예산을 다시 기다리지 않는지 검증합니다.
    /// 테스트는 제한 시간 판정 후 활성 작업을 반드시 해제하여 다른 테스트에 실행 상태가 남지 않도록 합니다.
    /// </summary>
    [Fact]
    public async Task DispatcherDoesNotSpendHandoffBudgetAgainAfterRegistryTimesOut()
    {
        var registry = new WorkerExecutionRegistry();
        Assert.True(registry.TryEnter(_ => Task.FromResult(false), out var execution));
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Worker:ShutdownGraceSeconds"] = "1",
                ["Worker:ShutdownHandoffSeconds"] = "4"
            }).Build();
        using var dispatcher = new WorkerTaskDispatcher(
            NullLogger<WorkerTaskDispatcher>.Instance, null!, registry, null!, configuration, TimeProvider.System);
        const string activeKey = "종료-예산-검증";
        var marked = (bool)Invoke(dispatcher, "TryMarkActive", activeKey)!;
        Assert.True(marked);
        Task? shutdown = null;
        try
        {
            shutdown = dispatcher.StopAsync(CancellationToken.None);
            // 종료 조정기는 유예 1초와 인계 4초를 사용하므로 정상 경로는 5초 안에 끝납니다.
            // 잘못된 구현이 인계 예산을 다시 사용하면 약 9초가 필요하므로 8초 제한을 넘습니다.
            // 정상 경로의 제한 시간을 넉넉히 두어 자체 호스팅 Runner의 스케줄링 지연이 테스트를 오판하지 않게 합니다.
            await shutdown.WaitAsync(TimeSpan.FromSeconds(8));
            Assert.False(registry.TryEnter(_ => Task.FromResult(true), out _));
        }
        finally
        {
            execution!.Dispose();
            Invoke(dispatcher, "RemoveActive", activeKey);
            if (shutdown is not null)
                await shutdown.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }

    private static object? Invoke(WorkerTaskDispatcher dispatcher, string method, string key) =>
        typeof(WorkerTaskDispatcher).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(dispatcher, [key]);
}
