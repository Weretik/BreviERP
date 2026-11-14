namespace Reference.Application.Features.GarmentAccessory.Commands.DeleteGarmentAccessory;

public sealed class DeleteGarmentAccessoryValidator : AbstractValidator<DeleteGarmentAccessoryCommand>
{
    public DeleteGarmentAccessoryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор фурнітури має бути більшим за 0.");
    }
}
