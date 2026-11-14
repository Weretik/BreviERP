using Crm.Api.Access;

namespace Crm.Api.Endpoints.Quotes;

/// <summary>
/// Gmail of staff for sending КП (R-15). The Director or Admin connects anyone, a manager only themselves; the person
/// signs in to their Gmail on the Google page and allows sending only.
/// </summary>
internal static class MailboxesEndpoints
{
    public static IEndpointRouteBuilder MapMailboxesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/crm/mailboxes").RequireAuthorization(CrmAccess.QuotesView)
            .WithTags("Mailboxes");
        ConnectCrmMailboxEndpoint.Map(group);
        DisconnectCrmMailboxEndpoint.Map(group);
        CrmMailboxGoogleCallbackEndpoint.Map(group);
        return app;
    }
}
