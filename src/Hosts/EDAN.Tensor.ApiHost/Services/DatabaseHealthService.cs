using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.ApiHost.Services;

/// <summary>API Host가 사용하는 모든 로컬 데이터베이스의 연결과 기본 스키마를 확인합니다.</summary>
public sealed class DatabaseHealthService(
    IServiceScopeFactory scopeFactory,
    SqliteDatabaseHealthProbe contentDatabase)
{
    /// <summary>Platform·Worker·DataSet 메타데이터와 DataSet 콘텐츠 데이터베이스 상태를 반환합니다.</summary>
    /// <param name="cancellationToken">상태 확인을 중단할 취소 토큰입니다.</param>
    /// <returns>각 데이터베이스의 연결 가능 여부와 전체 상태를 반환합니다.</returns>
    public async Task<DatabaseHealthSnapshot> CheckAsync(CancellationToken cancellationToken = default)
    {
        var databases = new Dictionary<string, bool>(StringComparer.Ordinal)
        {
            ["Platform"] = false,
            ["Worker"] = false,
            ["DataSet"] = false,
            ["DataSetContent"] = false
        };

        await using var scope = scopeFactory.CreateAsyncScope();
        databases["Platform"] = await CheckPlatformAsync(
            scope.ServiceProvider.GetRequiredService<PlatformDbContext>(), cancellationToken);
        databases["Worker"] = await CheckWorkerAsync(
            scope.ServiceProvider.GetRequiredService<WorkerDbContext>(), cancellationToken);
        databases["DataSet"] = await CheckDataSetAsync(
            scope.ServiceProvider.GetRequiredService<DataSetDbContext>(), cancellationToken);
        databases["DataSetContent"] = await contentDatabase.CanConnectAsync(cancellationToken);

        return new DatabaseHealthSnapshot(databases);
    }

    private static async Task<bool> CheckPlatformAsync(
        PlatformDbContext db, CancellationToken cancellationToken)
    {
        try
        {
            if (!await db.Database.CanConnectAsync(cancellationToken))
                return false;
            await db.Tenants.Select(x => x.Id).Take(1).ToListAsync(cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> CheckWorkerAsync(
        WorkerDbContext db, CancellationToken cancellationToken)
    {
        try
        {
            if (!await db.Database.CanConnectAsync(cancellationToken))
                return false;
            await db.WorkerTasks.Select(x => x.Id).Take(1).ToListAsync(cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> CheckDataSetAsync(
        DataSetDbContext db, CancellationToken cancellationToken)
    {
        try
        {
            if (!await db.Database.CanConnectAsync(cancellationToken))
                return false;
            await db.DataSets.Select(x => x.Id).Take(1).ToListAsync(cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>DataSet 콘텐츠 SQLite 데이터베이스의 연결만 확인합니다.</summary>
public sealed class SqliteDatabaseHealthProbe
{
    private readonly string connectionString;

    /// <summary>콘텐츠 데이터베이스 연결 문자열을 저장합니다.</summary>
    /// <param name="connectionString">확인할 SQLite 연결 문자열입니다.</param>
    public SqliteDatabaseHealthProbe(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        this.connectionString = connectionString;
    }

    /// <summary>콘텐츠 데이터베이스에 연결하고 간단한 읽기 명령을 실행합니다.</summary>
    /// <param name="cancellationToken">상태 확인을 중단할 취소 토큰입니다.</param>
    /// <returns>연결과 읽기가 모두 성공하면 true를 반환합니다.</returns>
    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1;";
            return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) == 1;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>데이터베이스별 연결 결과와 전체 상태를 표현합니다.</summary>
public sealed record DatabaseHealthSnapshot(IReadOnlyDictionary<string, bool> Databases)
{
    /// <summary>모든 데이터베이스가 연결되고 기본 스키마를 읽을 수 있는지 나타냅니다.</summary>
    public bool CanConnect => Databases.Values.All(value => value);

    /// <summary>외부 상태 확인 API가 사용할 연결 상태 문자열입니다.</summary>
    public string Status => CanConnect ? "Connected" : "Disconnected";

    /// <summary>연결 상태를 신호등 형식으로 표현합니다.</summary>
    public string Indicator => CanConnect ? "Green" : "Red";
}
