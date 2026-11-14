using BuildingBlocks.Application.DependencyInjection;
using Reminders.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reminders.Infrastructure.DependencyInjection;

namespace Reminders.Api.DependencyInjection;

public static class RemindersModuleExtensions
{
    /// <summary>Registers the Reminders module: Application, Infrastructure, its Contracts implementations and controllers.</summary>
    public static IServiceCollection AddRemindersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRemindersInfrastructure(configuration);
        services.AddModuleValidators(typeof(RemindersApplicationAssemblyMarker).Assembly);
        services.AddControllers().AddApplicationPart(typeof(RemindersApiAssemblyMarker).Assembly);
        return services;
    }
}
