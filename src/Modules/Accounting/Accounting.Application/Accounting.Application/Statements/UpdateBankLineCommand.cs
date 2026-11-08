using Accounting.Application.Abstractions;
using Accounting.Application.Contracts;
using Accounting.Application.Counterparties;
using Accounting.Application.Lookup;
using Accounting.Application.Transactions;
using Accounting.Domain.Categories;
using Accounting.Domain.Statements;

namespace Accounting.Application.Statements;

/// <summary>
/// The director's choice for one line of a statement that is not booked yet: category, counterparty or a one-off
/// name, description, note and whether to book it. The bank data stay as they came.
/// </summary>
public sealed record UpdateBankLineCommand(
    Guid StatementId,
    Guid LineId,
    Guid? CategoryId,
    FinanceCounterpartyLinkInput? Counterparty,
    string? OneoffName,
    string? Description,
    string? Comment,
    bool Excluded) : ICommand<Result<BankStatementLineResponse>>;

public sealed class UpdateBankLineCommandValidator : AbstractValidator<UpdateBankLineCommand>
{
    public UpdateBankLineCommandValidator()
    {
        RuleFor(x => x.StatementId).NotEmpty();
        RuleFor(x => x.LineId).NotEmpty();
        RuleFor(x => x.Counterparty!).SetValidator(new FinanceCounterpartyLinkInputValidator()).When(x => x.Counterparty is not null);
    }
}

public sealed class UpdateBankLineCommandHandler(IAccountingDbContext db, IAccountingCustomerReader customers,
    IAccountingSupplierReader suppliers)
    : ICommandHandler<UpdateBankLineCommand, Result<BankStatementLineResponse>>
{
    public async ValueTask<Result<BankStatementLineResponse>> Handle(UpdateBankLineCommand command,
        CancellationToken cancellationToken)
    {
        var statementId = BankStatementId.From(command.StatementId);
        var statement = await db.BankStatements.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == statementId, cancellationToken);
        if (statement is null) return Result.NotFound(StatementMessages.NotFound);
        var lineId = BankStatementLineId.From(command.LineId);
        var line = await db.BankStatementLines
            .FirstOrDefaultAsync(x => x.Id == lineId && x.StatementId == statementId, cancellationToken);
        if (line is null) return Result.NotFound(StatementMessages.LineNotFound);
        statement.EnsurePreview();

        FinanceCategory? category = null;
        if (command.CategoryId is { } categoryId)
        {
            var id = FinanceCategoryId.From(categoryId);
            category = await db.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (category is null) return Result.NotFound(AccountingMessages.CategoryNotFound);
        }

        var readers = new CounterpartyReaders(db, customers, suppliers);
        var (counterparty, gone) = await CounterpartyStates.ResolveAsync(readers, command.Counterparty?.ToLink(), cancellationToken);
        if (gone is not null) return Result.NotFound(gone);

        line.Edit(category, counterparty, command.OneoffName, command.Description, command.Comment, command.Excluded);
        await db.SaveChangesAsync(cancellationToken);
        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        return Result.Success(BankStatementLineResponse.From(line, names));
    }
}
