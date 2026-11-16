using System.Text;
using EDAN.Tensor.Worker.Infrastructure.Storage;
using Xunit;

namespace EDAN.Tensor.Worker.Infrastructure.Tests;

public sealed class WorkerResultStoreTests
{
    [Fact]
    public async Task FileStoreRoundTripsBytesAndRejectsOtherTenantReference()
    {
        var root = Path.Combine(Path.GetTempPath(), "edan-worker-result-test-" + Guid.NewGuid().ToString("N"));
        var tenantId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        try
        {
            var store = new FileSystemWorkerResultStore(root);
            await using var input = new MemoryStream(Encoding.UTF8.GetBytes("worker bytes"));
            var metadata = await store.SaveAsync(tenantId, taskId, "text/plain; charset=utf-8", input);

            Assert.Equal(tenantId, metadata.TenantId);
            Assert.Equal(taskId, metadata.TaskId);
            Assert.Equal(12, metadata.Length);
            Assert.StartsWith("worker-file:", metadata.ResultReference, StringComparison.Ordinal);
            Assert.Null(await store.GetMetadataAsync(Guid.NewGuid(), taskId, metadata.ResultReference));

            await using var output = await store.OpenReadAsync(metadata);
            Assert.NotNull(output);
            using var reader = new StreamReader(output!);
            Assert.Equal("worker bytes", await reader.ReadToEndAsync());
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
