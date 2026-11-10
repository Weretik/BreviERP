namespace Reference.Application.Features.FabricColor.Delete;

/// <summary>Removes a colour from the list; items and stock lines keep their colour text.</summary>
public sealed record DeleteFabricColorCommand(int Id) : ICommand<Result>;

public sealed class DeleteFabricColorCommandValidator : AbstractValidator<DeleteFabricColorCommand>
{
    public DeleteFabricColorCommandValidator() =>
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Ідентифікатор кольору має бути більшим за 0.");
}
