using Reference.Application.Features.PpeMarkupRule.Commands.CreatePpeMarkupRule;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.PpeMarkupRules.CreatePpeMarkupRule;

internal static class CreatePpeMarkupRuleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).Produces<int>()
            .RequireAuthorization("access:pricing-parameters");

    private static async Task<IResult> HandleAsync(CreatePpeMarkupRuleRequest request, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new CreatePpeMarkupRuleCommand(new CreatePpeMarkupRuleCommandRequest(request.Name,
            request.RetailMarkup,
            request.WholesaleMarkup, request.LargeWholesaleMarkup, request.PriceNote, request.AppliesTo,
                request.SortOrder)), cancellationToken)).ToHttpResult();
}

/// <summary>A new PPE markup rule; markups are fractions (0.45 = 45 %).</summary>
public sealed record CreatePpeMarkupRuleRequest(
    string Name,
    decimal RetailMarkup,
    decimal WholesaleMarkup,
    decimal LargeWholesaleMarkup,
    string? PriceNote = null,
    string? AppliesTo = null,
    int SortOrder = 0);
