namespace Reference.Application.Features.GarmentAccessory.Delete.Validators;

public sealed class DeleteGarmentAccessoryCommandValidator : AbstractValidator<DeleteGarmentAccessoryCommand>
{
    public DeleteGarmentAccessoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
