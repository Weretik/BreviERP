using Reference.Api.Endpoints.PpeMarkupRules.CreatePpeMarkupRule;
using Reference.Api.Endpoints.PpeMarkupRules.DeletePpeMarkupRule;
using Reference.Api.Endpoints.PpeMarkupRules.GetPpeMarkupRules;
using Reference.Api.Endpoints.PpeMarkupRules.UpdatePpeMarkupRule;

namespace Reference.Api.Endpoints.PpeMarkupRules;

/// <summary>PPE markup rules of the Calculator sheet «Налаштування цін» (right table).</summary>
internal static class PpeMarkupRulesEndpoints
{
    public static IEndpointRouteBuilder MapPpeMarkupRulesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reference/ppe-markup-rules").WithTags("PpeMarkupRules");
        GetPpeMarkupRulesEndpoint.Map(group);
        CreatePpeMarkupRuleEndpoint.Map(group);
        UpdatePpeMarkupRuleEndpoint.Map(group);
        DeletePpeMarkupRuleEndpoint.Map(group);
        return app;
    }
}
