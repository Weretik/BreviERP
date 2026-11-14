using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.CreateQuoteIntroTemplate;
using Crm.Application.Features.Quotes.Shared.Introductions;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.CreateCrmQuoteIntroTemplate;

internal static class CreateCrmQuoteIntroTemplateEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createCrmQuoteIntroTemplate");

    private static async Task<IResult> HandleAsync(CreateQuoteIntroTemplateInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        var result = await sender.Send(new CreateQuoteIntroTemplateCommand(actor, input), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/crm/quote-intro-templates/{result.Value.Id}", result.Value)
            : result.ToHttpResult();
    }
}
