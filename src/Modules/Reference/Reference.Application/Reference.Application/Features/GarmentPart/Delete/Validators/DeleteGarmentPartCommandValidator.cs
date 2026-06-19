namespace Reference.Application.Features.GarmentPart.Delete.Validators;

public sealed class DeleteGarmentPartCommandValidator : AbstractValidator<DeleteGarmentPartCommand>
{
    public DeleteGarmentPartCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор частини виробу має бути більшим за 0.");
    }
}
