namespace Reference.Application.Features.ProductSection.Commands.DeleteProductSection;

public sealed class DeleteProductSectionValidator : AbstractValidator<DeleteProductSectionCommand>
{
    public DeleteProductSectionValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор розділу має бути більшим за 0.");
    }
}
