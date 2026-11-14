using PricingParameterEntity = Reference.Domain.PricingParameters.PricingParameter;

namespace Reference.Application.Features.PricingParameter.Shared;

public sealed class PricingParameterFieldsValidator : AbstractValidator<IPricingParameterFields>
{
    public PricingParameterFieldsValidator()
    {
        RuleFor(x => x.Group)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Група параметра є обов'язковою.")
            .MaximumLength(PricingParameterEntity.GroupMaxLength).WithMessage("Група: не більше 100 символів.");

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Назва параметра є обов'язковою.")
            .MaximumLength(PricingParameterEntity.NameMaxLength).WithMessage("Назва: не більше 200 символів.");

        RuleFor(x => x.Unit)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Одиниця виміру є обов'язковою.")
            .MaximumLength(PricingParameterEntity.UnitMaxLength).WithMessage("Одиниця виміру: не більше 30 символів.");

        RuleFor(x => x.Value)
            .NotNull().WithMessage("Вкажіть значення параметра.")
            .GreaterThanOrEqualTo(0).WithMessage("Значення не може бути від'ємним.")
            .LessThan(1_000_000_000).WithMessage("Значення занадто велике.")
            .When(x => !x.IsCalculated);

        RuleFor(x => x.Comment)
            .MaximumLength(PricingParameterEntity.CommentMaxLength).WithMessage("Коментар: не більше 1000 символів.");

        RuleFor(x => x.SortOrder)
            .InclusiveBetween(0, 100_000).WithMessage("Порядок має бути від 0 до 100000.");
    }
}
