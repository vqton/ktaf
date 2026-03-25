using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using AMS.Infrastructure.Data;

public class AMSDbContextFactory : IDesignTimeDbContextFactory<AMSDbContext>
{
    public AMSDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AMSDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=15432;Database=ams_db;Username=postgres;Password=123456");
        return new AMSDbContext(optionsBuilder.Options);
    }
}