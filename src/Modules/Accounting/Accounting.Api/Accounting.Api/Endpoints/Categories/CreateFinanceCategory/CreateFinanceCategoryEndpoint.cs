using Accounting.Application.Features.Categories.Shared;
using Accounting.Api.Access;
using Accounting.Application.Features.Categories.Commands.CreateFinanceCategory;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Categories.CreateFinanceCategory;

/// <summary>409 when the same level already has this name.</summary>
internal static class CreateFinanceCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createFinanceCategory")
            .WithSummary("«Додати категорію», or a subcategory with parentId")
            .WithDescription("«Додати категорію», or a subcategory with parentId. A subcategory must match its root in direction and scope; only two levels.")
            .Produces<FinanceCategoryResponse>();

    private static async Task<IResult> HandleAsync(FinanceCategoryInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new CreateFinanceCategoryCommand(input.Name, input.Direction, input.Scope,
            input.ParentId, input.Note, input.SortOrder, AccountingAccess.By(user)), cancellationToken)).ToHttpResult();
}
