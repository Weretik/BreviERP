using Accounting.Application.Features.Categories.Shared;
using Accounting.Api.Access;
using Accounting.Application.Features.Categories.Commands.UpdateFinanceCategory;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Categories.UpdateFinanceCategory;

internal static class UpdateFinanceCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:guid}", HandleAsync).WithName("updateFinanceCategory")
            .WithSummary("Rename a category or change its note and order")
            .Produces<FinanceCategoryResponse>();

    private static async Task<IResult> HandleAsync(Guid id, FinanceCategoryUpdateInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateFinanceCategoryCommand(id, input.Name, input.Note, input.SortOrder,
            input.Version, AccountingAccess.By(user)), cancellationToken)).ToHttpResult();
}
