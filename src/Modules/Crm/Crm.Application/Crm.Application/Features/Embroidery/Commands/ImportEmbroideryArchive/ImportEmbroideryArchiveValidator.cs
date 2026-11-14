using Crm.Domain.Embroidery.Calculation;
using Crm.Domain.Embroidery.Saved;
using Crm.Domain.Embroidery.Settings;
using Crm.Application.Features.Embroidery.Commands.SaveEmbroideryCalculation;
using Crm.Application.Features.Embroidery.Commands.SaveEmbroiderySettings;
using Crm.Application.Features.Embroidery.Queries.CalculateEmbroidery;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;

namespace Crm.Application.Features.Embroidery.Commands.ImportEmbroideryArchive;

public sealed class ImportEmbroideryArchiveValidator : AbstractValidator<ImportEmbroideryArchiveCommand>
{
    public const int MaxRows = 2000;

    public ImportEmbroideryArchiveValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Rows).NotNull().Must(x => x is { Count: > 0 and <= MaxRows })
            .WithMessage($"У файлі має бути від 1 до {MaxRows} розрахунків.");
        RuleForEach(x => x.Rows).NotNull().ChildRules(row =>
        {
            row.RuleFor(x => x.LegacyId).NotEmpty().MaximumLength(EmbroiderySavedCalculation.LegacyIdMaxLength);
            row.RuleFor(x => x.Name).MaximumLength(SaveEmbroideryCalculationValidator.RawNameMaxLength);
            row.RuleFor(x => x.CreatedByEmail).MaximumLength(EmbroiderySavedCalculation.PersonNameMaxLength);
            row.RuleFor(x => x.CreatedAt).NotEqual(default(DateTimeOffset));
            row.RuleFor(x => x.StitchRate).GreaterThan(0);
            row.RuleFor(x => x.Calculation).NotNull().SetValidator(new CalculateEmbroideryInputValidator());
            row.RuleFor(x => x.Settings).NotNull().SetValidator(new SaveEmbroiderySettingsInputValidator(EmbroiderySettingsRules.LegacyMarkupCount));
            row.RuleFor(x => x.Prices).NotNull().Must(x => x is { Count: > 0 })
                .WithMessage("Немає збережених цін старого розрахунку.");
        });
    }
}
