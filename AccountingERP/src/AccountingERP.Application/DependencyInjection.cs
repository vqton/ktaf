using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;

namespace AccountingERP.Application;

/// <summary>
/// Dependency injection extensions cho Application layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR - CQRS
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }
}
