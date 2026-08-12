namespace Catalog.Application.Features.Product.Update.Validators;

public sealed class ReplaceProductCommandValidator : AbstractValidator<ReplaceProductCommand>
{
    public ReplaceProductCommandValidator()
    {
        RuleFor(x => x.Id).InclusiveBetween(1, 1_000_000_000);
        RuleFor(x => x.Request).NotNull();
    }
}
