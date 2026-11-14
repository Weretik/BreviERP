using Content.Application.Abstractions;
using Content.Application.Features.ProductPages.Shared;

namespace Content.Application.Features.ProductPages.Commands.DeleteContentProduct;

public sealed class DeleteContentProductValidator : AbstractValidator<DeleteContentProductCommand>
{
    public DeleteContentProductValidator() => RuleFor(x => x.ProductId).GreaterThan(0);
}
