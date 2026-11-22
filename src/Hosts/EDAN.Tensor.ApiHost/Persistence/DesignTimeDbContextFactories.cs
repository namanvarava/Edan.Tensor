using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using EDAN.Tensor.PostgreSqlMigrations;
using EDAN.Tensor.SqlServerMigrations;
using EDAN.Tensor.OracleMigrations;
using Oracle.EntityFrameworkCore;

namespace EDAN.Tensor.ApiHost.Persistence;

/// <summary>
/// EF Core 명령이 실행될 때 각 데이터베이스 컨텍스트를 동일한 공급자 설정으로 생성합니다.
/// </summary>
public static class DesignTimeDbContextFactorySupport
{
    /// <summary>
    /// 프로세스 환경 변수에서 공급자와 연결 문자열을 읽어 컨텍스트 옵션을 구성합니다.
    /// </summary>
    public static DbContextOptionsBuilder<TContext> Configure<TContext>(
        DbContextOptionsBuilder<TContext> options,
        string connectionName,
        string defaultSqliteFile,
        string migrationAssembly,
        string historyTable)
        where TContext : DbContext
    {
        var provider = Environment.GetEnvironmentVariable("Database__Provider") ?? "Sqlite";
        var connection = Environment.GetEnvironmentVariable($"ConnectionStrings__{connectionName}");

        if (string.IsNullOrWhiteSpace(connection))
        {
            connection = $"Data Source={Path.Combine(AppContext.BaseDirectory, defaultSqliteFile)}";
        }

        if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)
            || provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
        {
            options.UseNpgsql(connection, npgsql =>
            {
                // PostgreSQL migrations are hosted by ApiHost so SQLite and PostgreSQL
                // can keep independent migration histories and type mappings.
                npgsql.MigrationsAssembly(typeof(PostgreSqlMigrationsMarker).Assembly.FullName!);
                npgsql.MigrationsHistoryTable(historyTable);
            });
            return options;
        }

        if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase)
            || provider.Equals("MSSQL", StringComparison.OrdinalIgnoreCase)
            || provider.Equals("SQL Server", StringComparison.OrdinalIgnoreCase))
        {
            options.UseSqlServer(connection, sqlServer =>
            {
                sqlServer.MigrationsAssembly(typeof(SqlServerMigrationsMarker).Assembly.FullName!);
                sqlServer.MigrationsHistoryTable(historyTable);
            });
            return options;
        }

        if (provider.Equals("Oracle", StringComparison.OrdinalIgnoreCase))
        {
            options.UseOracle(connection, oracle =>
            {
                oracle.MigrationsAssembly(typeof(OracleMigrationsMarker).Assembly.FullName!);
                oracle.MigrationsHistoryTable(historyTable);
            });
            return options;
        }

        var sqlite = new SqliteConnectionStringBuilder(connection);
        if (!Path.IsPathRooted(sqlite.DataSource) && sqlite.DataSource != ":memory:")
        {
            sqlite.DataSource = Path.Combine(AppContext.BaseDirectory, sqlite.DataSource);
        }

        options.UseSqlite(sqlite.ConnectionString, sqliteOptions =>
        {
            sqliteOptions.MigrationsAssembly(migrationAssembly);
        });
        return options;
    }
}

/// <summary>
/// Platform Migration 스크립트 생성에 필요한 컨텍스트를 제공합니다.
/// </summary>
public sealed class PlatformDbContextFactory : IDesignTimeDbContextFactory<PlatformDbContext>
{
    /// <summary>
    /// Platform 컨텍스트를 생성합니다.
    /// </summary>
    public PlatformDbContext CreateDbContext(string[] args)
    {
        var options = DesignTimeDbContextFactorySupport.Configure(
            new DbContextOptionsBuilder<PlatformDbContext>(),
            "PlatformDatabase",
            "edan-tensor-platform.db",
            typeof(PlatformDbContext).Assembly.FullName!,
            "__EFMigrationsHistory_Platform");
        return new PlatformDbContext(options.Options);
    }
}

/// <summary>
/// Worker Migration 스크립트 생성에 필요한 컨텍스트를 제공합니다.
/// </summary>
public sealed class WorkerDbContextFactory : IDesignTimeDbContextFactory<WorkerDbContext>
{
    /// <summary>
    /// Worker 컨텍스트를 생성합니다.
    /// </summary>
    public WorkerDbContext CreateDbContext(string[] args)
    {
        var options = DesignTimeDbContextFactorySupport.Configure(
            new DbContextOptionsBuilder<WorkerDbContext>(),
            "WorkerDatabase",
            "edan-worker.db",
            typeof(WorkerDbContext).Assembly.FullName!,
            "__EFMigrationsHistory_Worker");
        return new WorkerDbContext(options.Options);
    }
}

/// <summary>
/// DataSet Migration 스크립트 생성에 필요한 컨텍스트를 제공합니다.
/// </summary>
public sealed class DataSetDbContextFactory : IDesignTimeDbContextFactory<DataSetDbContext>
{
    /// <summary>
    /// DataSet 컨텍스트를 생성합니다.
    /// </summary>
    public DataSetDbContext CreateDbContext(string[] args)
    {
        var options = DesignTimeDbContextFactorySupport.Configure(
            new DbContextOptionsBuilder<DataSetDbContext>(),
            "DataSetDatabase",
            "edan-tensor-dataset.db",
            typeof(DataSetDbContext).Assembly.FullName!,
            "__EFMigrationsHistory_DataSet");
        return new DataSetDbContext(options.Options);
    }
}
