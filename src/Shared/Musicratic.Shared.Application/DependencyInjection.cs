using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Musicratic.Shared.Application.Behaviors;

namespace Musicratic.Shared.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedApplication(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
