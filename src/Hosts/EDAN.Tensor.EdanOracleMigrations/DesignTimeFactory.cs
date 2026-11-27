using EDAN.Tensor.Edan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace EDAN.Tensor.EdanOracleMigrations;
public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<EdanDbContext>
{
    public EdanDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__EdanDatabase") ?? "User Id=edan;Password=edan;Data Source=localhost:1521/FREEPDB1";
        var b = new DbContextOptionsBuilder<EdanDbContext>();
        b.UseOracle(cs, o => o.MigrationsAssembly(typeof(DesignTimeFactory).Assembly.FullName));
        return new EdanDbContext(b.Options);
    }
}
