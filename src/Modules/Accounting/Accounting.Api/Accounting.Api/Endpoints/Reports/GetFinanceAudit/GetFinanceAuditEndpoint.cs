using Accounting.Application.Features.Audit.Queries.GetFinanceAudit;
using Accounting.Domain.Audit;

namespace Accounting.Api.Endpoints.Reports.GetFinanceAudit;

internal static class GetFinanceAuditEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFinanceAudit");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, QueryEnum<AuditEntity>? entity = null,
        QueryEnum<AuditAction>? action = null, int page = 1, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetFinanceAuditQuery((entity?.Value), (action?.Value), page),
            cancellationToken)).ToHttpResult();
}
