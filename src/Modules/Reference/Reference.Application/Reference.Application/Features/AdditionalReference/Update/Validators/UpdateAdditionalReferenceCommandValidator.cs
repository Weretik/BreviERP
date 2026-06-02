using Reference.Application.Features.AdditionalReference.Update;

namespace Reference.Application.Features.AdditionalReference.Update.Validators;

public sealed class UpdateAdditionalReferenceCommandValidator : AbstractValidator<UpdateAdditionalReferenceCommand>
{
    private static readonly string[] AllowedUnits = ["шт.", "грн.", "%"];

    public UpdateAdditionalReferenceCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Request.Key)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Request.Value)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Request.Unit)
            .NotEmpty()
            .MaximumLength(5)
            .Must(unit => AllowedUnits.Contains(unit))
            .WithMessage("Unit must be one of: шт., грн., %");

        RuleFor(x => x.Request.Description)
            .MaximumLength(255);
    }
}
