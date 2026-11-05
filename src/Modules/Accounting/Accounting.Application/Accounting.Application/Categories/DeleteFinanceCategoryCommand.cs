using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;

namespace Accounting.Application.Categories;

/// <summary>
/// «Видалити» a category; a root goes with its subcategories, each with its own log entry. Stage 1 has no movements
/// yet; from stage 2 a category in use is only switched off and keeps its id.
/// </summary>
public sealed record DeleteFinanceCategoryCommand(Guid Id, int Version, string By) : ICommand<Result>;

public sealed class DeleteFinanceCategoryCommandValidator : AbstractValidator<DeleteFinanceCategoryCommand>
{
    public DeleteFinanceCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).NotEmpty().MaximumLength(AuditEntry.ByMaxLength);
    }
}

public sealed class DeleteFinanceCategoryCommandHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<DeleteFinanceCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteFinanceCategoryCommand command, CancellationToken cancellationToken)
    {
        var id = FinanceCategoryId.From(command.Id);
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null) return Result.NotFound(AccountingMessages.CategoryNotFound);
        if (category.Version != command.Version) return Result.Conflict(AccountingMessages.Stale);

        var now = clock.GetUtcNow();
        var children = await db.Categories.Where(x => x.ParentId == id).ToListAsync(cancellationToken);
        foreach (var child in children.Append(category))
        {
            AuditTrail.Add(db, AuditEntity.Category, child.Id.Value, AuditAction.Delete, command.By, now,
                AuditTrail.Snapshot(child), null);
            db.Categories.Remove(child);
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(AccountingMessages.Stale);
        }

        return Result.Success();
    }
}
