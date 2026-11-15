using Accounting.Api.Access;
using Accounting.Application.Features.Attachments.Commands.DeleteFinanceAttachment;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Attachments.DeleteFinanceAttachment;

internal static class DeleteFinanceAttachmentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("api/accounting/attachments/{id:guid}", HandleAsync).WithName("deleteFinanceAttachment")
            .WithSummary("Delete finance attachment")
            .WithDescription("«Видалити»: the file leaves the operation and the storage; the change log keeps its name (AttachmentDelete of the Transaction).");

    private static async Task<IResult> HandleAsync(Guid id, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteFinanceAttachmentCommand(id, AccountingAccess.By(user)),
            cancellationToken)).ToNoContentHttpResult();
}
