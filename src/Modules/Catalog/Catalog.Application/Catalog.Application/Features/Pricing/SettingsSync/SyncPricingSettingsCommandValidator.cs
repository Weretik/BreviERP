using Catalog.Application.Features.Pricing.Validation;

namespace Catalog.Application.Features.Pricing.SettingsSync;

public sealed class SyncPricingSettingsCommandValidator : AbstractValidator<SyncPricingSettingsCommand>
{
    public SyncPricingSettingsCommandValidator()
    {
        RuleFor(x => x.Parameters).NotNull().WithMessage("Немає параметрів для синхронізації.");
        RuleFor(x => x.PpeRules).NotNull().WithMessage("Немає правил націнки для синхронізації.");
        RuleForEach(x => x.PpeRules).NotNull().WithMessage("Порожнє правило націнки.");
        RuleFor(x => x.Actor).ValidActor();
    }
}
