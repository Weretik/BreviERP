namespace Reference.Application.Features.GarmentPartOperation.Delete.Validators;

public sealed class DeleteGarmentPartOperationCommandValidator : AbstractValidator<DeleteGarmentPartOperationCommand>
{
    public DeleteGarmentPartOperationCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор операції частини виробу має бути більшим за 0.");
    }
}
