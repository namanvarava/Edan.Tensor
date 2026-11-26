using EDAN.Tensor.Edan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EDAN.Tensor.EdanMigrations;

public sealed class DesignTimeEdanDbContextFactory : IDesignTimeDbContextFactory<EdanDbContext>
{
    public EdanDbContext CreateDbContext(string[] args)
    {
        var provider = Environment.GetEnvironmentVariable("Database__Provider") ?? "Sqlite";
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__EdanDatabase")
            ?? "Data Source=edan-design.db";
        var options = new DbContextOptionsBuilder<EdanDbContext>();

        if (provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            options.UseNpgsql(connection, o => o.MigrationsAssembly(typeof(EdanMigrationsMarker).Assembly.FullName));
        else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            options.UseSqlServer(connection, o => o.MigrationsAssembly(typeof(EdanMigrationsMarker).Assembly.FullName));
        else if (provider.Equals("Oracle", StringComparison.OrdinalIgnoreCase))
            options.UseOracle(connection, o => o.MigrationsAssembly(typeof(EdanMigrationsMarker).Assembly.FullName));
        else
            options.UseSqlite(connection, o => o.MigrationsAssembly(typeof(EdanMigrationsMarker).Assembly.FullName));

        return new EdanDbContext(options.Options);
    }
}
