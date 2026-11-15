using Accounting.Application.Features.Audit.Queries.GetFinanceAudit;
using Accounting.Domain.Audit;
using BuildingBlocks.Api.Errors;
using BuildingBlocks.Api.Http;

namespace Accounting.Api.Endpoints.Reports.GetFinanceAudit;

internal static class GetFinanceAuditEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFinanceAudit")
            .WithSummary("«Журнал змін» (stage 8): changes of all finance records, newest first, 50 per page")
            .Produces<FinanceAuditPageResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, QueryEnum<AuditEntity>? entity = null,
        QueryEnum<AuditAction>? action = null, int page = 1, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetFinanceAuditQuery((entity?.Value), (action?.Value), page),
            cancellationToken)).ToHttpResult();
}
