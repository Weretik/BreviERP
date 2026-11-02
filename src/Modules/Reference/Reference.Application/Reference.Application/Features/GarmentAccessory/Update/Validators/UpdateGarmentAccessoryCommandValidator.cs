using Reference.Application.Features.GarmentAccessory.Shared.Validators;

namespace Reference.Application.Features.GarmentAccessory.Update.Validators;

public sealed class UpdateGarmentAccessoryCommandValidator : AbstractValidator<UpdateGarmentAccessoryCommand>
{
    public UpdateGarmentAccessoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор фурнітури має бути більшим за 0.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на оновлення фурнітури не може бути порожнім.");

        When(x => x.Request is not null, () => RuleFor(x => x.Request).SetValidator(new GarmentAccessoryFieldsValidator()));
    }
}
