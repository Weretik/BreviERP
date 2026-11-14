using Reference.Application.Features.GarmentAccessory.Shared;

namespace Reference.Application.Features.GarmentAccessory.Commands.CreateGarmentAccessory;

public sealed class CreateGarmentAccessoryValidator : AbstractValidator<CreateGarmentAccessoryCommand>
{
    public CreateGarmentAccessoryValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на створення фурнітури не може бути порожнім.");

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Id)
                .GreaterThan(0)
                .WithMessage("Ідентифікатор фурнітури має бути більшим за 0.");

            RuleFor(x => x.Request).SetValidator(new GarmentAccessoryFieldsValidator());
        });
    }
}
