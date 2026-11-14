using Crm.Api.Access;
using Crm.Api.Endpoints.Leads.CreateCrmLeadCallResult;
using Crm.Api.Endpoints.Leads.EditCrmLeadCallResult;
using Crm.Api.Endpoints.Leads.EditCrmLeadCategory;
using Crm.Api.Endpoints.Leads.GetCrmLeadCategories;
using Crm.Api.Endpoints.Leads.GetCrmLeadStats;
using Crm.Api.Endpoints.Leads.GetCrmLeadStatsCallResults;
using Crm.Api.Endpoints.Leads.SyncCrmLeads;

namespace Crm.Api.Endpoints.Leads;

/// <summary>
/// «Адміністрування › Ліди» (LEAD-001): «Зведення» (the old «Сводка»), «Довідники» (the old «Справочники») and
/// «Синхронізувати зараз». By default Director and Admin.
/// </summary>
internal static class LeadStatsEndpoints
{
    public static IEndpointRouteBuilder MapLeadStatsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/crm/lead-stats").RequireAuthorization(CrmAccess.LeadStats)
            .WithTags("LeadStats");
        GetCrmLeadStatsEndpoint.Map(group);
        GetCrmLeadStatsCallResultsEndpoint.Map(group);
        CreateCrmLeadCallResultEndpoint.Map(group);
        EditCrmLeadCallResultEndpoint.Map(group);
        GetCrmLeadCategoriesEndpoint.Map(group);
        EditCrmLeadCategoryEndpoint.Map(group);
        SyncCrmLeadsEndpoint.Map(group);
        return app;
    }
}
