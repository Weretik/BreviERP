using Accounting.Application.Features.History.Queries.GetFinanceHistory;
using Accounting.Domain.Audit;

namespace Accounting.Api.Endpoints.Categories;

internal static class GetFinanceCategoryHistoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:guid}/history", HandleAsync).WithName("getFinanceCategoryHistory");

    private static async Task<IResult> HandleAsync(Guid id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetFinanceHistoryQuery(AuditEntity.Category, id), cancellationToken)).ToHttpResult();
}
