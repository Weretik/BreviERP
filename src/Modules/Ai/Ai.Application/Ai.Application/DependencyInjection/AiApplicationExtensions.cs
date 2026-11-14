using Ai.Application.Features.Calls.Shared;
using Ai.Application.Features.Settings.Shared;

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
