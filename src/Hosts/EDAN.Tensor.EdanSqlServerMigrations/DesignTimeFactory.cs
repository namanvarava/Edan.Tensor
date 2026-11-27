using EDAN.Tensor.Edan.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace EDAN.Tensor.EdanSqlServerMigrations;
public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<EdanDbContext>
{
    public EdanDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__EdanDatabase") ?? "Server=localhost;Database=edan;User Id=sa;Password=Password1!;TrustServerCertificate=True";
        var b = new DbContextOptionsBuilder<EdanDbContext>();
        b.UseSqlServer(cs, o => o.MigrationsAssembly(typeof(DesignTimeFactory).Assembly.FullName));
        return new EdanDbContext(b.Options);
    }
}
