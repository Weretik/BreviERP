using Catalog.Application.Features.Pricing.Shared.Validation;

namespace Catalog.Application.Features.Pricing.Commands.SyncPricingSettings;

public sealed class SyncPricingSettingsValidator : AbstractValidator<SyncPricingSettingsCommand>
{
    public SyncPricingSettingsValidator()
    {
        RuleFor(x => x.Parameters).NotNull().WithMessage("Немає параметрів для синхронізації.");
        RuleFor(x => x.PpeRules).NotNull().WithMessage("Немає правил націнки для синхронізації.");
        RuleForEach(x => x.PpeRules).NotNull().WithMessage("Порожнє правило націнки.");
        RuleFor(x => x.Actor).ValidActor();
    }
}
