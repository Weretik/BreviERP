using Crm.Api.Access;
using Crm.Application.Features.Leads.Commands.SyncLeads;

namespace Crm.Api.Endpoints.Leads;

/// <summary>«Синхронізувати зараз»: reads the sheet now; 503 when Google cannot be read, 422 when the sheet has an
/// unexpected shape (both with the message also saved as the last sync error), 409 when a sync runs.</summary>
internal static class SyncCrmLeadsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("sync", HandleAsync).WithName("syncCrmLeads")
            .RequireAuthorization(CrmAccess.LeadSync);

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SyncLeadsCommand(), cancellationToken);
        return result.Status switch
        {
            ResultStatus.Unavailable => Results.Json(statusCode: StatusCodes.Status503ServiceUnavailable,
                data: new { errors = result.Errors }),
            ResultStatus.Error => Results.UnprocessableEntity(new { errors = result.Errors }),
            _ => result.ToHttpResult(),
        };
    }
}
