using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DTE.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
        });
        
        services.AddValidatorsFromAssembly(assembly);
        
        return services;
    }
}
