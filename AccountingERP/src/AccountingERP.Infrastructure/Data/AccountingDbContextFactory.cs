using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AccountingERP.Infrastructure.Data;

/// <summary>
/// Factory để tạo DbContext trong runtime với multi-database support
/// </summary>
public class AccountingDbContextFactory : IDesignTimeDbContextFactory<AccountingDbContext>
{
    public AccountingDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var provider = configuration.GetValue<string>("Database:Provider") ?? "sqlite";
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=accounting.db";

        var optionsBuilder = new DbContextOptionsBuilder<AccountingDbContext>();
        
        ConfigureDatabase(optionsBuilder, provider, connectionString);

        return new AccountingDbContext(optionsBuilder.Options);
    }

    public static void ConfigureDatabase(DbContextOptionsBuilder optionsBuilder, string provider, string connectionString)
    {
        switch (provider.ToLowerInvariant())
        {
            case "sqlserver":
                optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly("AccountingERP.Infrastructure");
                    sqlOptions.EnableRetryOnFailure(3);
                });
                break;
            
            case "sqlite":
            default:
                optionsBuilder.UseSqlite(connectionString, sqliteOptions =>
                {
                    sqliteOptions.MigrationsAssembly("AccountingERP.Infrastructure");
                });
                break;
        }
    }
}
