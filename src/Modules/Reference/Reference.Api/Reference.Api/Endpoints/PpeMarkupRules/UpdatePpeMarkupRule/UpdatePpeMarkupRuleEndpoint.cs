using Reference.Application.Features.PpeMarkupRule.Commands.UpdatePpeMarkupRule;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.PpeMarkupRules.UpdatePpeMarkupRule;

internal static class UpdatePpeMarkupRuleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync)
            .WithName("updatePpeMarkupRule")
            .WithSummary("Update a PPE markup rule")
            .RequireAuthorization("access:pricing-parameters");

    private static async Task<IResult> HandleAsync(int id, UpdatePpeMarkupRuleRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdatePpeMarkupRuleCommand(id,
            new UpdatePpeMarkupRuleCommandRequest(request.RetailMarkup,
            request.WholesaleMarkup, request.LargeWholesaleMarkup, request.PriceNote, request.AppliesTo,
                request.SortOrder)), cancellationToken)).ToHttpResult();
}

/// <summary>Editable fields of a PPE markup rule; the name cannot change.</summary>
public sealed record UpdatePpeMarkupRuleRequest(
    decimal RetailMarkup,
    decimal WholesaleMarkup,
    decimal LargeWholesaleMarkup,
    string? PriceNote = null,
    string? AppliesTo = null,
    int SortOrder = 0);
