namespace Reference.Application.Features.Fabric.Commands.DeleteFabric;

public sealed class DeleteFabricValidator : AbstractValidator<DeleteFabricCommand>
{
    public DeleteFabricValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор тканини має бути більшим за 0.");
    }
}
