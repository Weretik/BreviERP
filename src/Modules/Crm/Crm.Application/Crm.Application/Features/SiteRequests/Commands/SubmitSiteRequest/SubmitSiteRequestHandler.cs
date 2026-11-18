using BuildingBlocks.Domain;
using BuildingBlocks.Application.Idempotency;
using BuildingBlocks.Application.Outbox;
using Crm.Domain.SiteRequests;
using Crm.Application.Features.SiteRequests.Shared;
using Crm.Application.Features.SiteRequests.Shared.Notify;
using Crm.Application.Abstractions.SiteRequests;
using BuildingBlocks.Application;

namespace Crm.Application.Features.SiteRequests.Commands.SubmitSiteRequest;

public sealed class SubmitSiteRequestHandler(
    ISiteRequestStore store, ISiteRequestFileStorage files, IOutboxDispatcher outbox, TimeProvider clock)
    : ICommandHandler<SubmitSiteRequestCommand, Result<SiteRequestReceivedResponse>>
{
    public async ValueTask<Result<SiteRequestReceivedResponse>> Handle(SubmitSiteRequestCommand command,
        CancellationToken cancellationToken)
    {
        // The same form sent again (a double press or a retry): the request is already in. The file content is not
        // hashed; its name is.
        var requestKey = RequestKeys.Of(command.RequestId,
            new { Form = command with { RequestId = null, File = null }, FileName = command.File?.FileName });
        if (await SavedAsync(requestKey, cancellationToken) is { } saved) return saved;

        var now = clock.GetUtcNow();
        string? fileUrl = null;
        if (command.File is { } file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var key = $"/site-requests/{now:yyyy-MM}/{Guid.NewGuid():N}{extension}";
            fileUrl = await files.UploadAsync(file.Content, key, ContentType(file.ContentType), cancellationToken);
        }

        var created = SiteRequest.Create(command.Phone, command.Workwear, command.Quantity, command.Comment,
            command.Locale ?? "uk", Path.GetFileName(command.File?.FileName), fileUrl, now, requestKey);
        if (created.IsFailure) return created.Error!.ToResult<SiteRequestReceivedResponse>();
        var request = created.Value;

        // The id comes from the database, so the request is saved first; one transaction keeps both or neither.
        await using var transaction = await store.Database.BeginTransactionAsync(cancellationToken);
        store.SiteRequests.Add(request);
        try
        {
            await store.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException) when (command.RequestId is not null)
        {
            // Both copies arrived at once and the other one was saved first.
            await transaction.RollbackAsync(CancellationToken.None);
            store.SiteRequests.Entry(request).State = EntityState.Detached;
            if (await SavedAsync(requestKey, cancellationToken) is { } other) return other;
            throw;
        }

        var message = OutboxMessage.Create(NotifySiteRequestOutboxHandler.MessageType,
            new NotifySiteRequestPayload(request.Id.Value), now);
        store.OutboxMessages.Add(message);
        await store.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await outbox.DispatchAsync([message.Id], CancellationToken.None);
        return Result.Success(new SiteRequestReceivedResponse(request.Id.Value));
    }

    /// <summary>The answer for a request saved under the id, a conflict when the id came with other data, or null.</summary>
    private async Task<Result<SiteRequestReceivedResponse>?> SavedAsync(RequestKey? request, CancellationToken cancellationToken)
    {
        if (request is not { } key) return null;
        var saved = await store.SiteRequests.AsNoTracking().Where(x => x.RequestId == key.Id)
            .Select(x => new { Id = x.Id.Value, x.RequestHash }).SingleOrDefaultAsync(cancellationToken);
        if (saved is null) return null;
        return key.Matches(saved.RequestHash)
            ? Result.Success(new SiteRequestReceivedResponse(saved.Id))
            : RequestKeys.Conflict<SiteRequestReceivedResponse>();
    }

    private static string ContentType(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "application/octet-stream" : value;
}
