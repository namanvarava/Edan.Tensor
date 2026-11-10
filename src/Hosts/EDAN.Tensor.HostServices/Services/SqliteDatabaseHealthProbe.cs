using Microsoft.Data.Sqlite;

namespace EDAN.Tensor.HostServices.Services;

/// <summary>SQLite 콘텐츠 데이터베이스의 연결만 확인합니다.</summary>
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
