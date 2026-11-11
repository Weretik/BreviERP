using System.Net;
using System.Text;
using Crm.Application.SiteRequests.Abstractions;
using Crm.Application.SiteRequests.Contracts;
using Crm.Domain.SiteRequests;

namespace Crm.Application.SiteRequests.Submit;

/// <summary>
/// A request from the «Заявка» form of the public site (LEAD-002 SC-01): saves it with the file on the CDN, then the
/// bot posts it to the chosen groups. A Telegram failure does not lose the request: it is saved with the reason.
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
    ISiteRequestStore store, ISiteRequestFileStorage files, ISiteRequestNotifier notifier, SiteRequestLinks links,
    TimeProvider clock, ILogger<SubmitSiteRequestCommandHandler> logger)
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

        var request = new SiteRequest(command.Phone!, command.Workwear, command.Quantity, command.Comment,
            command.Locale ?? "uk", Path.GetFileName(command.File?.FileName), fileUrl, now);
        store.SiteRequests.Add(request);
        await store.SaveChangesAsync(cancellationToken);

        string? error;
        try
        {
            error = await notifier.NotifyAsync(Message(request, links), cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Site request {Id} was not posted to Telegram", request.Id);
            error = "Telegram недоступний.";
        }

        request.Notified(error is null ? clock.GetUtcNow() : null, error);
        await store.SaveChangesAsync(CancellationToken.None);
        return Result.Success(new SiteRequestReceivedResponse(request.Id));
    }

    private static string ContentType(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "application/octet-stream" : value;

    /// <summary>The Telegram card of a new request, in Telegram HTML.</summary>
    public static string Message(SiteRequest request, SiteRequestLinks links)
    {
        static string E(string value) => WebUtility.HtmlEncode(value);
        var text = new StringBuilder();
        text.Append("🆕 <b>Заявка з сайту</b> №").Append(request.Id).Append('\n');
        text.Append("📞 <b>").Append(E(request.Phone)).Append("</b>\n");
        if (request.Workwear is not null) text.Append("Що пошити: ").Append(E(request.Workwear)).Append('\n');
        if (request.Quantity is not null) text.Append("Кількість: ").Append(E(request.Quantity)).Append('\n');
        if (request.Comment is not null) text.Append("Коментар: ").Append(E(request.Comment)).Append('\n');
        if (request.FileUrl is not null)
            text.Append("Файл: <a href=\"").Append(E(request.FileUrl)).Append("\">")
                .Append(E(request.FileName ?? "файл")).Append("</a>\n");
        if (request.Locale == "ru") text.Append("Сторінка російською\n");
        text.Append("<a href=\"").Append(E(links.List)).Append("\">Відкрити в адмінці</a>");
        return text.ToString();
    }
}
