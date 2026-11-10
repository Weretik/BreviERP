using FabricColorEntity = Reference.Domain.FabricColors.Entities.FabricColor;

namespace Reference.Application.Features.FabricColor.Shared;

public sealed class FabricColorFieldsValidator : AbstractValidator<FabricColorFields>
{
    public FabricColorFieldsValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Назва кольору є обов'язковою.")
            .MaximumLength(FabricColorEntity.NameMaxLength).WithMessage("Назва кольору: не більше 80 символів.");

        RuleFor(x => x.SortOrder)
            .InclusiveBetween(0, 100_000).WithMessage("Порядок має бути від 0 до 100000.");
    }
}
