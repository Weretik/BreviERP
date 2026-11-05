using Ai.Application.Calls;
using Ai.Application.Settings;

namespace Ai.Application.DependencyInjection;

public static class AiApplicationExtensions
{
    public static IServiceCollection AddAiApplication(this IServiceCollection services)
    {
        services.AddScoped<AiSettingsStore>();
        services.AddScoped<AiCaller>();
        return services;
    }
}
