using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Categories.Shared;

namespace Accounting.Application.Features.Categories.Commands.CreateFinanceCategory;

public sealed class CreateFinanceCategoryHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<CreateFinanceCategoryCommand, Result<FinanceCategoryResponse>>
{
    public const string WrongParent = "Підкатегорія має бути того ж типу й області обліку, що й коренева категорія.";

    public async ValueTask<Result<FinanceCategoryResponse>> Handle(CreateFinanceCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        DomainResult<FinanceCategory> created;
        if (command.ParentId is { } parentGuid)
        {
            var parent = await db.Categories.FirstOrDefaultAsync(x => x.Id == FinanceCategoryId.From(parentGuid),
                cancellationToken);
            if (parent is null) return Result.NotFound(AccountingMessages.ParentNotFound);
            if (parent.Direction != command.Direction || parent.Scope != command.Scope)
                return Result.Invalid(new ValidationError(nameof(command.ParentId), WrongParent));
            created = FinanceCategory.CreateChild(FinanceCategoryId.New(), command.Name, parent, command.Note,
                command.SortOrder, now);
        }
        else
        {
            created = FinanceCategory.CreateRoot(FinanceCategoryId.New(), command.Name, command.Direction, command.Scope,
                command.Note, command.SortOrder, now);
        }

        if (created.IsFailure) return created.Error!.ToResult<FinanceCategoryResponse>();
        var category = created.Value;

        if (await CategoryNames.TakenAsync(db, category.Name, category.Direction, category.Scope, category.ParentId, null,
                cancellationToken))
            return Result.Conflict(AccountingMessages.DuplicateCategory);

        db.Categories.Add(category);
        AuditTrail.Add(db, AuditEntity.Category, category.Id.Value, AuditAction.Create, command.By, now, null,
            AuditTrail.Snapshot(category));
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(FinanceCategoryResponse.From(category));
    }
}
