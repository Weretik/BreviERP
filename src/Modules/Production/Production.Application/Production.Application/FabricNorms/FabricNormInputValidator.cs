using Production.Application.Contracts;
using Production.Domain.FabricNorms;

namespace Production.Application.FabricNorms;

internal sealed class FabricNormInputValidator : AbstractValidator<FabricNormInput>
{
    public FabricNormInputValidator()
    {
        RuleFor(x => x.ToValues()).Must(x => x.All().All(FabricNormValues.IsValidMeters))
            .WithMessage(FabricNormMessages.ValueInvalid).OverridePropertyName("norm");
        RuleFor(x => x.ToValues()).Must(x => x.HasMainForEveryAdditional)
            .WithMessage(FabricNormMessages.AdditionalWithoutMain).OverridePropertyName("norm");
        RuleFor(x => x.ToValues()).Must(x => !x.IsEmpty)
            .WithMessage(FabricNormMessages.Empty).OverridePropertyName("norm");
    }
}
