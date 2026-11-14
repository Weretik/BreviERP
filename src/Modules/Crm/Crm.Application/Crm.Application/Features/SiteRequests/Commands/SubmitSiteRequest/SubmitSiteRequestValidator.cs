using BuildingBlocks.Application.Outbox;
using Crm.Domain.SiteRequests;
using Crm.Application.Features.SiteRequests.Shared;
using Crm.Application.Features.SiteRequests.Shared.Notify;

namespace Crm.Application.Features.SiteRequests.Commands.SubmitSiteRequest;

public sealed class SubmitSiteRequestValidator : AbstractValidator<SubmitSiteRequestCommand>
{
    public const long MaxFileBytes = 10 * 1024 * 1024;

    /// <summary>The formats the form of the site accepts: a brief, a table, an archive or a layout.</summary>
    public static readonly IReadOnlyList<string> Extensions =
        [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".zip", ".rar", ".7z", ".jpg", ".jpeg", ".png", ".cdr", ".ai", ".eps", ".psd"];

    public SubmitSiteRequestValidator()
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
