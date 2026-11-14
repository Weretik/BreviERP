namespace Reference.Application.Features.FabricColor.Commands.DeleteFabricColor;

/// <summary>Removes a colour from the list; items and stock lines keep their colour text.</summary>
public sealed record DeleteFabricColorCommand(int Id) : ICommand<Result>;

public sealed class DeleteFabricColorValidator : AbstractValidator<DeleteFabricColorCommand>
{
    public DeleteFabricColorValidator() =>
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Ідентифікатор кольору має бути більшим за 0.");
}
