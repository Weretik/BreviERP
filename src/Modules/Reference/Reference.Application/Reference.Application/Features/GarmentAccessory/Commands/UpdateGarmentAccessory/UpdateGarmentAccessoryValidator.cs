using Reference.Application.Features.GarmentAccessory.Shared;

namespace Reference.Application.Features.GarmentAccessory.Commands.UpdateGarmentAccessory;

public sealed class UpdateGarmentAccessoryValidator : AbstractValidator<UpdateGarmentAccessoryCommand>
{
    public UpdateGarmentAccessoryValidator()
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
