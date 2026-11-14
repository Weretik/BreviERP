using BuildingBlocks.Application.Outbox;
using Crm.Application.SiteRequests.Abstractions;
using Crm.Application.SiteRequests.Contracts;
using Crm.Application.SiteRequests.Notify;
using Crm.Domain.SiteRequests;

namespace Crm.Application.SiteRequests.Submit;

/// <summary>
/// A request from the «Заявка» form of the public site (LEAD-002 SC-01): saves it with the file on the CDN and, in the
/// same transaction, the outbox message that posts it to the bot's groups (PLT-003 R-07). The post is tried at once;
/// a Telegram failure keeps the request with the reason and the outbox tries again.
/// </summary>
public sealed record SubmitSiteRequestCommand(
    string? Phone,
    string? Workwear,
    string? Quantity,
    string? Comment,
    string? Locale,
    SiteRequestFile? File) : ICommand<Result<SiteRequestReceivedResponse>>;

public sealed class SubmitSiteRequestCommandValidator : AbstractValidator<SubmitSiteRequestCommand>
{
    public const long MaxFileBytes = 10 * 1024 * 1024;

    /// <summary>The formats the form of the site accepts: a brief, a table, an archive or a layout.</summary>
    public static readonly IReadOnlyList<string> Extensions =
        [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".zip", ".rar", ".7z", ".jpg", ".jpeg", ".png", ".cdr", ".ai", ".eps", ".psd"];

    public SubmitSiteRequestCommandValidator()
    {
        RuleFor(x => x.Phone).Must(IsPhone).WithMessage(SiteRequestMessages.Phone);
        RuleFor(x => x.Workwear).MaximumLength(SiteRequest.WorkwearMaxLength);
        RuleFor(x => x.Quantity).MaximumLength(SiteRequest.QuantityMaxLength);
        RuleFor(x => x.Comment).MaximumLength(SiteRequest.CommentMaxLength);
        RuleFor(x => x.Locale).Must(x => x is null or "uk" or "ru");
        RuleFor(x => x.File!.Length).InclusiveBetween(1, MaxFileBytes).WithMessage(SiteRequestMessages.FileTooLarge)
            .When(x => x.File is not null);
        RuleFor(x => x.File!.FileName).Must(HasAllowedExtension).WithMessage(SiteRequestMessages.FileType)
            .When(x => x.File is not null);
    }

    /// <summary>A Ukrainian number as the site checks it: 10 digits from 0, or 12 from 380, other characters ignored.</summary>
    public static bool IsPhone(string? value)
    {
        if (value is null || value.Length > SiteRequest.PhoneMaxLength) return false;
        var digits = new string(value.Where(char.IsAsciiDigit).ToArray());
        return (digits.Length == 10 && digits[0] == '0') || (digits.Length == 12 && digits.StartsWith("380"));
    }

    public static bool HasAllowedExtension(string? name) =>
        name is not null && Extensions.Contains(Path.GetExtension(name).ToLowerInvariant());
}

public sealed class SubmitSiteRequestCommandHandler(
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
