using System.Reflection;
using FluentValidation;
using MediatR;
using SafePick.Application.Common.Behaviors;

namespace SafePick.Api.DependencyInjection;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = Assembly.Load("SafePick.Application");
        var infrastructureAssembly = Assembly.Load("SafePick.Infrastructure");

        services.AddMediatR(cfg =>
        {
            // Handlers may live in Application (use cases) OR in Infrastructure
            // (notification handlers, which call infrastructure services).
            // Register both assemblies so MediatR discovers all handlers.
            cfg.RegisterServicesFromAssembly(applicationAssembly);
            cfg.RegisterServicesFromAssembly(infrastructureAssembly);
        });

        services.AddValidatorsFromAssembly(applicationAssembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainEventDispatchBehavior<,>));

        return services;
    }
}