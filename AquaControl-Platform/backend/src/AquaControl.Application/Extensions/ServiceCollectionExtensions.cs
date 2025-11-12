using System.Reflection;
using AquaControl.Application.Common.Behaviors;
using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AquaControl.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Add pipeline behaviors
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Add FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}

