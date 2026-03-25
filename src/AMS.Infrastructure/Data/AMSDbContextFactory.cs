using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using AMS.Infrastructure.Data;

/// <summary>
/// Factory for creating AMSDbContext instances at design time (for EF Core migrations).
/// </summary>
public class AMSDbContextFactory : IDesignTimeDbContextFactory<AMSDbContext>
{
    /// <summary>
    /// Creates a new instance of AMSDbContext.
    /// </summary>
    /// <param name="args">Arguments provided by the design-time environment.</param>
    /// <returns>A new AMSDbContext instance.</returns>
    public AMSDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AMSDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=15432;Database=ams_db;Username=postgres;Password=123456");
        return new AMSDbContext(optionsBuilder.Options);
    }
}