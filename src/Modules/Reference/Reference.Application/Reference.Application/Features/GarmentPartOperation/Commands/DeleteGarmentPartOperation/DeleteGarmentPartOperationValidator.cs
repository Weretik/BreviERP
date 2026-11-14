namespace Reference.Application.Features.GarmentPartOperation.Commands.DeleteGarmentPartOperation;

public sealed class DeleteGarmentPartOperationValidator : AbstractValidator<DeleteGarmentPartOperationCommand>
{
    public DeleteGarmentPartOperationValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор операції частини виробу має бути більшим за 0.");
    }
}
