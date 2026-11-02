using Reference.Application.Features.GarmentPartOperation.Shared.Validators;

namespace Reference.Application.Features.GarmentPartOperation.Update.Validators;

public sealed class UpdateGarmentPartOperationCommandValidator : AbstractValidator<UpdateGarmentPartOperationCommand>
{
    public UpdateGarmentPartOperationCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор операції має бути більшим за 0.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на оновлення операції не може бути порожнім.");

        When(x => x.Request is not null, () =>
            RuleFor(x => x.Request).SetValidator(new GarmentPartOperationFieldsValidator()));
    }
}
