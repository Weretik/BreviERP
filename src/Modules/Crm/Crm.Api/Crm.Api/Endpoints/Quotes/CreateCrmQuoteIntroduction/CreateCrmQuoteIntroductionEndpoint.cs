using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.CreateQuoteIntroduction;
using Crm.Application.Features.Quotes.Shared.Introductions;

namespace Crm.Api.Endpoints.Quotes.CreateCrmQuoteIntroduction;

internal static class CreateCrmQuoteIntroductionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createCrmQuoteIntroduction");

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
