using Microsoft.AspNetCore.RateLimiting;
using Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;
using Purchasing.Application.Features.Register.Shared;
using BuildingBlocks.Api.Errors;

namespace Purchasing.Api.Endpoints.Register.SyncPurchaseInvoices;

/// <summary>«Оновити зараз»: 409 while another sync runs, 503 when Google cannot be read, 422 when the sheet breaks a rule.</summary>
internal static class SyncPurchaseInvoicesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("sync", HandleAsync).WithName("syncPurchaseInvoices").RequireRateLimiting("HeavyOperation")
            .WithSummary("«Оновити зараз»: reads the whole sheet, checks it, then applies it in one transaction")
            .WithDescription("«Оновити зараз»: reads the whole sheet, checks it, then applies it in one transaction. Safe to repeat; manual supplier links and notes are never changed. Rows and links missing from the sheet become inactive.").Produces<PurchaseSyncResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SyncPurchaseRegisterCommand(), cancellationToken);
        return result.ToHttpResultWithRefusal();
    }
}
