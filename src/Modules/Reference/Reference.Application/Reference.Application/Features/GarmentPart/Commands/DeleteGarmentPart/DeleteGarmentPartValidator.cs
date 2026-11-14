namespace Reference.Application.Features.GarmentPart.Commands.DeleteGarmentPart;

public sealed class DeleteGarmentPartValidator : AbstractValidator<DeleteGarmentPartCommand>
{
    public DeleteGarmentPartValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор частини виробу має бути більшим за 0.");
    }
}
