using Reference.Application.Features.ProductSection.Commands.CreateProductSection;

namespace Reference.Api.Endpoints.ProductSections;

internal static class CreateProductSectionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).Produces<int>()
            .RequireAuthorization("access:product-sections");

    private static async Task<IResult> HandleAsync(CreateProductSectionRequest request, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new CreateProductSectionCommand(new CreateProductSectionCommandRequest(request.Type,
            request.Name,
            request.IsActive, request.Comment, request.SortOrder)), cancellationToken)).ToHttpResult();
}

/// <summary>A new product section; <paramref name="Type"/> is <c>Sewing</c> or <c>Ppe</c>.</summary>
public sealed record CreateProductSectionRequest(
    string Type,
    string Name,
    bool IsActive = true,
    string? Comment = null,
    int SortOrder = 0);
