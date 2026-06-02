namespace Reference.Application.Features.GarmentPartOperation.Delete.Validators;

public sealed class DeleteGarmentPartOperationCommandValidator : AbstractValidator<DeleteGarmentPartOperationCommand>
{
    public DeleteGarmentPartOperationCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
