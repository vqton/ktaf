using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AccountingERP.Application.Interfaces;
using AccountingERP.Infrastructure.Data;
using AccountingERP.Infrastructure.Repositories;

namespace AccountingERP.Infrastructure;

/// <summary>
/// Dependency injection extensions cho Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database configuration
        var provider = configuration.GetValue<string>("Database:Provider") ?? "sqlite";
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=accounting.db";

        services.AddDbContext<AccountingDbContext>(options =>
        {
            AccountingDbContextFactory.ConfigureDatabase(options, provider, connectionString);
        });

        // Repositories
        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();

        return services;
    }
}
