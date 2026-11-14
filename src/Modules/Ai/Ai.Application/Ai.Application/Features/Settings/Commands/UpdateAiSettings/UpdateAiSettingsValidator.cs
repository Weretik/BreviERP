using Ai.Application.Abstractions;
using Ai.Domain.Settings;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Application.Features.Settings.Commands.UpdateAiSettings;

public sealed class UpdateAiSettingsValidator : AbstractValidator<UpdateAiSettingsCommand>
{
    public UpdateAiSettingsValidator()
    {
        RuleFor(x => x.FastModel).Must(AiModelName.IsValid).WithMessage(AiModelName.Message);
        RuleFor(x => x.StrongModel).Must(AiModelName.IsValid).WithMessage(AiModelName.Message);
        RuleFor(x => x.FastInputPrice).InclusiveBetween(0, AiModelPrice.Max).PrecisionScale(10, 4, true);
        RuleFor(x => x.FastOutputPrice).InclusiveBetween(0, AiModelPrice.Max).PrecisionScale(10, 4, true);
        RuleFor(x => x.StrongInputPrice).InclusiveBetween(0, AiModelPrice.Max).PrecisionScale(10, 4, true);
        RuleFor(x => x.StrongOutputPrice).InclusiveBetween(0, AiModelPrice.Max).PrecisionScale(10, 4, true);
        RuleFor(x => x.MonthlyLimitUsd).InclusiveBetween(0, AiSettings.MonthlyLimitMax).PrecisionScale(10, 2, true)
            .WithMessage($"Ліміт на місяць — від 0 до {AiSettings.MonthlyLimitMax} $.");
        RuleFor(x => x.Version).GreaterThanOrEqualTo(0);
        RuleFor(x => x.By).NotEmpty().MaximumLength(AiSettings.UpdatedByMaxLength);
    }
}
