using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using POSShopTicketing.Application.Common.Behaviours;
using POSShopTicketing.Application.Common.Interfaces;
using POSShopTicketing.Application.Common.Services;

namespace POSShopTicketing.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<ISlaCalculator, SlaCalculator>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });

        return services;
    }
}
