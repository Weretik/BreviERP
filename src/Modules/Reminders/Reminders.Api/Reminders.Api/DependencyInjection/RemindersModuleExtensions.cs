using Reminders.Api.Endpoints.Calendar;
using Reminders.Api.Endpoints.Chats;
using Reminders.Api.Endpoints.Occurrences;
using Reminders.Api.Endpoints.Tasks;
using Reminders.Api.Endpoints.Telegram;
using Reminders.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reminders.Infrastructure.DependencyInjection;
using BuildingBlocks.Application.Validation;

namespace Reminders.Api.DependencyInjection;

public static class RemindersModuleExtensions
{
    /// <summary>Registers the Reminders module: Application, Infrastructure, and its Contracts implementations.</summary>
    public static IServiceCollection AddRemindersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRemindersInfrastructure(configuration);
        services.AddModuleValidators(typeof(RemindersApplicationAssemblyMarker).Assembly);
        return services;
    }

    /// <summary>Maps the Reminders module's HTTP endpoints.</summary>
    public static IEndpointRouteBuilder MapRemindersEndpoints(this IEndpointRouteBuilder app) =>
        app.MapCalendarEndpoints().MapChatsEndpoints().MapOccurrencesEndpoints().MapTasksEndpoints()
            .MapTelegramEndpoints();
}
