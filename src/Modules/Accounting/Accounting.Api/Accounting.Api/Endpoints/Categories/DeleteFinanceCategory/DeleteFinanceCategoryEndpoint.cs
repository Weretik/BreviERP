using Accounting.Api.Access;
using Accounting.Application.Features.Categories.Commands.DeleteFinanceCategory;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Categories.DeleteFinanceCategory;

/// <summary>A root is deleted with its subcategories.</summary>
internal static class DeleteFinanceCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:guid}", HandleAsync).WithName("deleteFinanceCategory")
            .WithSummary("Delete a category; a root goes with its subcategories, each with its own log entry")
            .WithDescription("Delete a category; a root goes with its subcategories, each with its own log entry. When an operation (cancelled ones too) uses the category or a subcategory, the whole branch is switched off instead (active false, log action Deactivate).");

    private static async Task<IResult> HandleAsync(Guid id, ClaimsPrincipal user, [FromServices] ISender sender,
        int version = default, CancellationToken cancellationToken = default) =>
        (await sender.Send(new DeleteFinanceCategoryCommand(id, version, AccountingAccess.By(user)),
            cancellationToken)).ToNoContentHttpResult();
}
