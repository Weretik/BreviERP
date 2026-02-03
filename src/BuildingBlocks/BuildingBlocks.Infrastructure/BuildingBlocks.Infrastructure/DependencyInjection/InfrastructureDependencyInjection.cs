using BuildingBlocks.Application.Contracts;
using BuildingBlocks.Application.Notifications;
using BuildingBlocks.Infrastructure.DomainEvents;
using BuildingBlocks.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


namespace BuildingBlocks.Infrastructure.DependencyInjection;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Domain Events
        services.AddScoped<IDomainEventContext, EfDomainEventContext>();
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

        //Регистрация Fake Services
        services.AddScoped<IPermissionService, FakePermissionService>();
        services.AddScoped<ICurrentUserService, FakeCurrentUserService>();

        return services;
    }
}
