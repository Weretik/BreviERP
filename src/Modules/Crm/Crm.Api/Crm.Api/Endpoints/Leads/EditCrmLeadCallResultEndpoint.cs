using Crm.Application.Features.Leads.Commands.SaveLeadCallResult;
using Crm.Application.Features.Leads.Shared;

namespace Crm.Api.Endpoints.Leads;

internal static class EditCrmLeadCallResultEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("call-results/{id:long}", HandleAsync).WithName("editCrmLeadCallResult");

    private static async Task<IResult> HandleAsync(long id, SaveLeadCallResultInput input,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new SaveLeadCallResultCommand(id, input), cancellationToken)).ToHttpResult();
}
