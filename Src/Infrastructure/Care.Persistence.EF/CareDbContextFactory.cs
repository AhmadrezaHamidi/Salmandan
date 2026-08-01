using Microsoft.EntityFrameworkCore.Design;

namespace Care.Persistence.EF;

/// <summary>Design-time factory برای EF Core migrations</summary>
public sealed class CareDbContextFactory : IDesignTimeDbContextFactory<CareDbContext>
{
    public CareDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CareDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=Salmandan;Trusted_Connection=True;TrustServerCertificate=True;",
            sql => sql.MigrationsAssembly(typeof(CareDbContext).Assembly.FullName));

        return new CareDbContext(optionsBuilder.Options);
    }
}
