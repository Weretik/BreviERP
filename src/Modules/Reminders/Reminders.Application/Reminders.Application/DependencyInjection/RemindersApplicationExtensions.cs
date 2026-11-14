using Reminders.Application.Abstractions;
using Reminders.Application.Features.Dispatch.Shared;
using Reminders.Application.Features.Occurrences.Shared;
using Reminders.Application.Features.Telegram.Commands.HandleTelegramUpdate;

namespace Reminders.Application.DependencyInjection;

public static class RemindersApplicationExtensions
{
    /// <summary>Services shared by Host.Api (admin API, webhook) and Host.Seed (daily dispatch job).</summary>
    public static IServiceCollection AddRemindersApplication(this IServiceCollection services)
    {
        services.AddScoped<ReminderClock>();
        services.AddScoped<ReminderOccurrenceGenerator>();
        services.AddScoped<ReminderDispatcher>();
        services.AddScoped<ReminderButtonHandler>();
        services.AddScoped<ReminderChatTracker>();
        services.AddScoped<ReminderOccurrenceStatusChanger>();
        return services;
    }
}
