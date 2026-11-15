using Reference.Application.Features.PpeMarkupRule.Commands.DeletePpeMarkupRule;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.PpeMarkupRules.DeletePpeMarkupRule;

internal static class DeletePpeMarkupRuleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync)
            .WithName("deletePpeMarkupRule")
            .WithSummary("Delete a PPE markup rule")
            .RequireAuthorization("access:pricing-parameters");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeletePpeMarkupRuleCommand(id), cancellationToken)).ToHttpResult();
}
