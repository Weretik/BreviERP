namespace Reference.Application.Features.Fabric.Delete.Validators;

public sealed class DeleteFabricCommandValidator : AbstractValidator<DeleteFabricCommand>
{
    public DeleteFabricCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
