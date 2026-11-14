using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.ImportQuotes;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Quotes.ImportCrmQuotes;

internal static class ImportCrmQuotesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("importCrmQuotes");

    private static async Task<IResult> HandleAsync(ImportQuotesRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new ImportQuotesCommand(actor, request.DryRun, request.Quotes),
            cancellationToken)).ToHttpResult();
    }
}
