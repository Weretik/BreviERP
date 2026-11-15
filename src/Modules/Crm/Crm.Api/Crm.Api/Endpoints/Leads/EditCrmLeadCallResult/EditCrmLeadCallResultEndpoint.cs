using Crm.Application.Features.Leads.Commands.SaveLeadCallResult;
using Crm.Application.Features.Leads.Shared;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Leads.EditCrmLeadCallResult;

internal static class EditCrmLeadCallResultEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("call-results/{id:long}", HandleAsync).WithName("editCrmLeadCallResult")
            .WithSummary("Edit CRM lead call result")
            .WithDescription("Requires access:lead-stats (edit). Changes a result. Results are never deleted, only switched off; recorded calls keep the old name.")
            .Produces<LeadCallResultResponse>();

    private static async Task<IResult> HandleAsync(long id, SaveLeadCallResultInput input,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new SaveLeadCallResultCommand(id, input), cancellationToken)).ToHttpResult();
}
