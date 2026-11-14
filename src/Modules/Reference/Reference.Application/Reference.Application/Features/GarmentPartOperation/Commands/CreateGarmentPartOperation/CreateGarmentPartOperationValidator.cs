using Reference.Application.Features.GarmentPartOperation.Shared;

namespace Reference.Application.Features.GarmentPartOperation.Commands.CreateGarmentPartOperation;

public sealed class CreateGarmentPartOperationValidator : AbstractValidator<CreateGarmentPartOperationCommand>
{
    public CreateGarmentPartOperationValidator()
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
