using Crm.Application.Features.Leads.Commands.SaveLeadCallResult;
using Crm.Application.Features.Leads.Shared;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Leads.CreateCrmLeadCallResult;

/// <summary>Adding a result is a change of the references, so it needs edit (not create, which is the sync).</summary>
internal static class CreateCrmLeadCallResultEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("call-results", HandleAsync).WithName("createCrmLeadCallResult")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(SaveLeadCallResultInput input, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new SaveLeadCallResultCommand(null, input), cancellationToken)).ToHttpResult();
}
