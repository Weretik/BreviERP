using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Categories.Shared;

namespace Accounting.Application.Features.Categories.Commands.UpdateFinanceCategory;

public sealed class UpdateFinanceCategoryHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<UpdateFinanceCategoryCommand, Result<FinanceCategoryResponse>>
{
    public async ValueTask<Result<FinanceCategoryResponse>> Handle(UpdateFinanceCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == FinanceCategoryId.From(command.Id),
            cancellationToken);
        if (category is null) return Result.NotFound(AccountingMessages.CategoryNotFound);
        if (category.Version != command.Version) return Result.Conflict(AccountingMessages.Stale);
        if (await CategoryNames.TakenAsync(db, command.Name ?? string.Empty, category.Direction, category.Scope,
                category.ParentId, category.Id, cancellationToken))
            return Result.Conflict(AccountingMessages.DuplicateCategory);

        var now = clock.GetUtcNow();
        var before = AuditTrail.Snapshot(category);
        var updated = category.Update(command.Name, command.Note, command.SortOrder, now);
        if (updated.IsFailure) return updated.Error!.ToResult<FinanceCategoryResponse>();
        if (!updated.Value)
            return Result.Success(FinanceCategoryResponse.From(category));

        AuditTrail.Add(db, AuditEntity.Category, command.Id, AuditAction.Update, command.By, now, before,
            AuditTrail.Snapshot(category));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(AccountingMessages.Stale);
        }

        return Result.Success(FinanceCategoryResponse.From(category));
    }
}
