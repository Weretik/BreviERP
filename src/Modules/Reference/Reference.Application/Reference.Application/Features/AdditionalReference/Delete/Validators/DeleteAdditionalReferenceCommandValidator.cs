using Reference.Application.Features.AdditionalReference.Delete;

namespace Reference.Application.Features.AdditionalReference.Delete.Validators;

public sealed class DeleteAdditionalReferenceCommandValidator : AbstractValidator<DeleteAdditionalReferenceCommand>
{
    public DeleteAdditionalReferenceCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор додаткового довідника має бути більшим за 0.");
    }
}
