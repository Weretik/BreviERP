using Content.Application.Abstractions;
using Content.Domain.Redirects;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Commands.ImportOldSiteRedirects;

public sealed class ImportOldSiteRedirectsValidator : AbstractValidator<ImportOldSiteRedirectsCommand>
{
    public ImportOldSiteRedirectsValidator()
    {
        RuleFor(x => x.UpdatedBy).NotEmpty();
        RuleFor(x => x.Items).NotNull().Must(items => items is { Count: > 0 and <= ImportOldSiteRedirectsCommand.MaxItems })
            .WithMessage($"Файл має містити від 1 до {ImportOldSiteRedirectsCommand.MaxItems} адрес.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.From).NotEmpty().MaximumLength(ImportOldSiteRedirectsCommand.AddressMaxLength)
                .WithMessage("Адреса у файлі порожня або задовга.");
            item.RuleFor(x => x.To).MaximumLength(ImportOldSiteRedirectsCommand.AddressMaxLength)
                .WithMessage("Нова адреса у файлі задовга.");
        }).When(x => x.Items is not null);
    }
}
