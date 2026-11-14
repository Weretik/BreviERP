using Reference.Application.Features.GarmentPartOperation.Shared;

namespace Reference.Application.Features.GarmentPartOperation.Commands.UpdateGarmentPartOperation;

public sealed class UpdateGarmentPartOperationValidator : AbstractValidator<UpdateGarmentPartOperationCommand>
{
    public UpdateGarmentPartOperationValidator()
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
