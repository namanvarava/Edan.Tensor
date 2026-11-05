using EDAN.Tensor.Worker.Application.Execution;
using EDAN.Tensor.Worker.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EDAN.Tensor.Worker.Infrastructure.Tests;

/// <summary>작업 유형별 Handler 등록과 중복 방지 동작을 검증합니다.</summary>
public sealed class WorkerTaskHandlerRegistryTests
{
    [Fact]
    public void DependencyInjectionRegistrationResolvesHandlerByTaskType()
    {
        var services = new ServiceCollection();
        services.AddWorkerTaskHandler<TestHandler>("integration.test");
        using var provider = services.BuildServiceProvider();

        var resolver = provider.GetRequiredService<IWorkerTaskHandlerResolver>();
        Assert.IsType<TestHandler>(resolver.Resolve("integration.test"));
        Assert.Null(resolver.Resolve("unknown.task"));
    }

    [Fact]
    public void RegistryRejectsDuplicateTaskType()
    {
        var registry = new WorkerTaskHandlerRegistry();
        registry.Register("integration.test", new TestHandler());

        Assert.Throws<InvalidOperationException>(() =>
            registry.Register("integration.test", new TestHandler()));
    }

    private sealed class TestHandler : IWorkerTaskHandler
    {
        public Task<IWorkerTaskExecution> StartAsync(
            EDAN.Tensor.Worker.Domain.Model.WorkerTask task, CancellationToken cancellationToken) =>
            throw new NotSupportedException("등록 테스트에서는 Handler를 실행하지 않습니다.");
    }
}
