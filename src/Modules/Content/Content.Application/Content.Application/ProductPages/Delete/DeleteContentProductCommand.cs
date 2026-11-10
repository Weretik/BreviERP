using Content.Application.Abstractions;
using Content.Application.ProductPages.Shared;

namespace Content.Application.ProductPages.Delete;

/// <summary>Removes the site page of a product with its texts (CNT-002 R-06); the Catalog product stays.</summary>
public sealed record DeleteContentProductCommand(int ProductId) : ICommand<Result>;

public sealed class DeleteContentProductCommandValidator : AbstractValidator<DeleteContentProductCommand>
{
    public DeleteContentProductCommandValidator() => RuleFor(x => x.ProductId).GreaterThan(0);
}

public sealed class DeleteContentProductCommandHandler(IContentDbContext db)
    : ICommandHandler<DeleteContentProductCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteContentProductCommand command, CancellationToken cancellationToken)
    {
        var page = await db.ProductPages.Include(x => x.Texts)
            .FirstOrDefaultAsync(x => x.ProductId == command.ProductId, cancellationToken);
        if (page is null) return Result.NotFound(ContentProductMessages.PageNotFound);
        db.ProductPages.Remove(page);
        await db.SaveChangesAsync(cancellationToken);
        return Result.NoContent();
    }
}
