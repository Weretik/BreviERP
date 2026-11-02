using Reference.Application.Features.GarmentPartOperation.Shared.Validators;

namespace Reference.Application.Features.GarmentPartOperation.Create.Validators;

public sealed class CreateGarmentPartOperationCommandValidator : AbstractValidator<CreateGarmentPartOperationCommand>
{
    public CreateGarmentPartOperationCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на створення операції не може бути порожнім.");

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Id)
                .GreaterThan(0)
                .WithMessage("Ідентифікатор операції має бути більшим за 0.");

            RuleFor(x => x.Request).SetValidator(new GarmentPartOperationFieldsValidator());
        });
    }
}
