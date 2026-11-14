using Ai.Api.Endpoints.Settings.GetAiSettings;
using Ai.Api.Endpoints.Settings.RemoveAiKey;
using Ai.Api.Endpoints.Settings.SetAiKey;
using Ai.Api.Endpoints.Settings.TestAiConnection;
using Ai.Api.Endpoints.Settings.UpdateAiPrompt;
using Ai.Api.Endpoints.Settings.UpdateAiSettings;

namespace Ai.Api.Endpoints.Settings;

/// <summary>«Адміністрування → Налаштування ШІ»: the OpenAI key (write-only), models, prices, on/off and the monthly limit.</summary>
internal static class AiSettingsEndpoints
{
    public static IEndpointRouteBuilder MapAiSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/ai/settings").RequireAuthorization("RequireAdminRole").WithTags("AiSettings");
        GetAiSettingsEndpoint.Map(group);
        UpdateAiSettingsEndpoint.Map(group);
        SetAiKeyEndpoint.Map(group);
        RemoveAiKeyEndpoint.Map(group);
        UpdateAiPromptEndpoint.Map(group);
        TestAiConnectionEndpoint.Map(group);
        return app;
    }
}
