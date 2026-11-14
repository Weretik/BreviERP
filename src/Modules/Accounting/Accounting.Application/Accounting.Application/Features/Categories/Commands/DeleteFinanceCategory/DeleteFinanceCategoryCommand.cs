using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;

namespace Accounting.Application.Features.Categories.Commands.DeleteFinanceCategory;

/// <summary>
/// «Видалити» a category; a root goes with its subcategories, each with its own log entry. When any operation (cancelled
/// ones too) uses the category or one of its subcategories, the whole branch is only switched off
/// (<see cref="AuditAction.Deactivate"/>) and keeps its ids, so old operations still show it.
/// </summary>
public sealed record DeleteFinanceCategoryCommand(Guid Id, int Version, string By) : ICommand<Result>;

public sealed class DeleteFinanceCategoryValidator : AbstractValidator<DeleteFinanceCategoryCommand>
{
    public DeleteFinanceCategoryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).NotEmpty().MaximumLength(AuditEntry.ByMaxLength);
    }
}

public sealed class DeleteFinanceCategoryHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<DeleteFinanceCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteFinanceCategoryCommand command, CancellationToken cancellationToken)
    {
        var id = FinanceCategoryId.From(command.Id);
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null) return Result.NotFound(AccountingMessages.CategoryNotFound);
        if (category.Version != command.Version) return Result.Conflict(AccountingMessages.Stale);

        var now = clock.GetUtcNow();
        var branch = (await db.Categories.Where(x => x.ParentId == id).ToListAsync(cancellationToken))
            .Append(category).ToList();
        var ids = branch.Select(x => (FinanceCategoryId?)x.Id).ToList();
        // A default category of a counterparty counts as use too: the link must keep pointing at a record.
        var used = await db.Transactions.AnyAsync(x => ids.Contains(x.CategoryId), cancellationToken)
            || await db.Counterparties.AnyAsync(x => ids.Contains(x.DefaultCategoryId), cancellationToken);
        foreach (var item in branch)
        {
            var before = AuditTrail.Snapshot(item);
            if (used)
            {
                if (item.Deactivate(now))
                    AuditTrail.Add(db, AuditEntity.Category, item.Id.Value, AuditAction.Deactivate, command.By, now,
                        before, AuditTrail.Snapshot(item));
                continue;
            }

            AuditTrail.Add(db, AuditEntity.Category, item.Id.Value, AuditAction.Delete, command.By, now, before, null);
            db.Categories.Remove(item);
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
