using Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;
using Purchasing.Application.Features.Register.Shared;
using BuildingBlocks.Api.Errors;

namespace Purchasing.Api.Endpoints.Register.SyncPurchaseInvoices;

/// <summary>«Оновити зараз»: 409 while another sync runs, 503 when Google cannot be read, 422 when the sheet breaks a rule.</summary>
internal static class SyncPurchaseInvoicesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("sync", HandleAsync).WithName("syncPurchaseInvoices").Produces<PurchaseSyncResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SyncPurchaseRegisterCommand(), cancellationToken);
        return result.Status switch
        {
            ResultStatus.Unavailable => Results.Json(new { errors = result.Errors },
                statusCode: StatusCodes.Status503ServiceUnavailable),
            ResultStatus.Error => Results.UnprocessableEntity(new { errors = result.Errors }),
            _ => result.ToHttpResult(),
        };
    }
}
