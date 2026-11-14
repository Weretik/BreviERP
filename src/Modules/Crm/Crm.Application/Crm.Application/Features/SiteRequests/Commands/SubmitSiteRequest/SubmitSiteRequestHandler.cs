using BuildingBlocks.Application.Outbox;
using Crm.Domain.SiteRequests;
using Crm.Application.Features.SiteRequests.Shared;
using Crm.Application.Features.SiteRequests.Shared.Notify;
using Crm.Application.Abstractions.SiteRequests;

namespace Crm.Application.Features.SiteRequests.Commands.SubmitSiteRequest;

public sealed class SubmitSiteRequestHandler(
    ISiteRequestStore store, ISiteRequestFileStorage files, IOutboxDispatcher outbox, TimeProvider clock)
    : ICommandHandler<SubmitSiteRequestCommand, Result<SiteRequestReceivedResponse>>
{
    public async ValueTask<Result<SiteRequestReceivedResponse>> Handle(SubmitSiteRequestCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        string? fileUrl = null;
        if (command.File is { } file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var key = $"/site-requests/{now:yyyy-MM}/{Guid.NewGuid():N}{extension}";
            fileUrl = await files.UploadAsync(file.Content, key, ContentType(file.ContentType), cancellationToken);
        }

        var created = SiteRequest.Create(command.Phone, command.Workwear, command.Quantity, command.Comment,
            command.Locale ?? "uk", Path.GetFileName(command.File?.FileName), fileUrl, now);
        if (created.IsFailure) return created.Error!.ToResult<SiteRequestReceivedResponse>();
        var request = created.Value;

        // The id comes from the database, so the request is saved first; one transaction keeps both or neither.
        await using var transaction = await store.Database.BeginTransactionAsync(cancellationToken);
        store.SiteRequests.Add(request);
        await store.SaveChangesAsync(cancellationToken);
        var message = OutboxMessage.Create(NotifySiteRequestOutboxHandler.MessageType,
            new NotifySiteRequestPayload(request.Id), now);
        store.OutboxMessages.Add(message);
        await store.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await outbox.DispatchAsync([message.Id], CancellationToken.None);
        return Result.Success(new SiteRequestReceivedResponse(request.Id));
    }

    private static string ContentType(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "application/octet-stream" : value;
}
