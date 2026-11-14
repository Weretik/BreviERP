using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.CreateQuote;
using Crm.Application.Features.Quotes.Shared;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.CreateCrmQuote;

internal static class CreateCrmQuoteEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createCrmQuote");

    private static async Task<IResult> HandleAsync(SaveQuoteInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        var result = await sender.Send(new CreateQuoteCommand(actor, input), cancellationToken);
        return result.IsSuccess
            ? Results.CreatedAtRoute("getCrmQuote", new { id = result.Value.Id }, result.Value)
            : result.ToHttpResult();
    }
}
