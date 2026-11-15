using Crm.Application.Features.Quotes.Shared;
using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.CreateQuoteIntroduction;
using Crm.Application.Features.Quotes.Shared.Introductions;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.CreateCrmQuoteIntroduction;

internal static class CreateCrmQuoteIntroductionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createCrmQuoteIntroduction")
            .WithSummary("Create CRM quote introduction")
            .WithDescription("Creates a draft «КП для знайомства» (R-16): one page per season of the layout, the main model of the season first, then 3 more models of that season, one fabric each. Only the від 41 шт. price is shown; it comes from the Catalog. It is read, listed, sent and deleted through /api/crm/quotes.")
            .Produces<QuoteResponse>(StatusCodes.Status201Created);

    private static async Task<IResult> HandleAsync(SaveQuoteIntroductionInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        var result = await sender.Send(new CreateQuoteIntroductionCommand(actor, input), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/crm/quotes/{result.Value.Id}", result.Value)
            : result.ToHttpResult();
    }
}
