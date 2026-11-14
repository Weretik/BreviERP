using Accounting.Api.Access;
using Accounting.Application.Features.Categories.Commands.DeleteFinanceCategory;

namespace Accounting.Api.Endpoints.Categories.DeleteFinanceCategory;

/// <summary>A root is deleted with its subcategories.</summary>
internal static class DeleteFinanceCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:guid}", HandleAsync).WithName("deleteFinanceCategory");

    private static async Task<IResult> HandleAsync(Guid id, ClaimsPrincipal user, [FromServices] ISender sender,
        int version = default, CancellationToken cancellationToken = default) =>
        (await sender.Send(new DeleteFinanceCategoryCommand(id, version, AccountingAccess.By(user)),
            cancellationToken)).ToNoContentHttpResult();
}
