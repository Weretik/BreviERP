namespace Reference.Application.Features.ProductSection.Delete.Validators;

public sealed class DeleteProductSectionCommandValidator : AbstractValidator<DeleteProductSectionCommand>
{
    public DeleteProductSectionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор розділу має бути більшим за 0.");
    }
}
