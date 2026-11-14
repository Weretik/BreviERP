using Crm.Api.Access;
using Crm.Api.Endpoints.Leads.GetCrmLead;
using Crm.Api.Endpoints.Leads.GetCrmLeadCallResults;
using Crm.Api.Endpoints.Leads.GetCrmLeads;
using Crm.Api.Endpoints.Leads.GetCrmLeadsMyDay;
using Crm.Api.Endpoints.Leads.RecordCrmLeadCall;
using Crm.Api.Endpoints.Leads.ReleaseCrmLead;
using Crm.Api.Endpoints.Leads.TakeCrmLead;

namespace Crm.Api.Endpoints.Leads;

/// <summary>
/// «Менеджер сервісу › Ліди» (LEAD-001): «Мій день» and «Усі ліди». Everyone with the page sees every lead and takes
/// free ones; only the owner records calls; Director and Admin may give back anybody's lead.
/// </summary>
internal static class LeadsEndpoints
{
    public static IEndpointRouteBuilder MapLeadsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/crm/leads").RequireAuthorization(CrmAccess.Leads)
            .WithTags("Leads");
        GetCrmLeadsEndpoint.Map(group);
        GetCrmLeadsMyDayEndpoint.Map(group);
        GetCrmLeadCallResultsEndpoint.Map(group);
        GetCrmLeadEndpoint.Map(group);
        TakeCrmLeadEndpoint.Map(group);
        ReleaseCrmLeadEndpoint.Map(group);
        RecordCrmLeadCallEndpoint.Map(group);
        return app;
    }
}
