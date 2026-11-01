using Reminders.Application.Abstractions;
using Reminders.Application.Dispatch;
using Reminders.Application.Occurrences;
using Reminders.Application.Telegram.HandleUpdate;

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
