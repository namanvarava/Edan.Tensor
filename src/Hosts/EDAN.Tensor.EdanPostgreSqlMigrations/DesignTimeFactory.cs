using EDAN.Tensor.Edan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace EDAN.Tensor.EdanPostgreSqlMigrations;
public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<EdanDbContext>
{
    public EdanDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__EdanDatabase") ?? "Host=localhost;Database=edan;Username=edan;Password=edan";
        var b = new DbContextOptionsBuilder<EdanDbContext>();
        b.UseNpgsql(cs, o => o.MigrationsAssembly(typeof(DesignTimeFactory).Assembly.FullName));
        return new EdanDbContext(b.Options);
    }
}
