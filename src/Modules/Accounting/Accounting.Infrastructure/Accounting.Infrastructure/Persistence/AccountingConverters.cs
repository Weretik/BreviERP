using Accounting.Domain.Accounts;
using Accounting.Domain.Attachments;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Statements;
using Accounting.Domain.Transactions;
using Accounting.Domain.Transfers;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Accounting.Domain.Statements.ValueObjects;

namespace Accounting.Infrastructure.Persistence;

internal static class AccountingConverters
{
    public static readonly ValueConverter<FinanceAccountId, Guid> AccountId = new(id => id.Value, v => FinanceAccountId.From(v));
    public static readonly ValueConverter<FinanceCategoryId, Guid> CategoryId = new(id => id.Value, v => FinanceCategoryId.From(v));
    public static readonly ValueConverter<FinanceTransactionId, Guid> TransactionId =
        new(id => id.Value, v => FinanceTransactionId.From(v));
    public static readonly ValueConverter<FinanceCounterpartyId, Guid> CounterpartyId =
        new(id => id.Value, v => FinanceCounterpartyId.From(v));
    public static readonly ValueConverter<FinanceAttachmentId, Guid> AttachmentId =
        new(id => id.Value, v => FinanceAttachmentId.From(v));
    public static readonly ValueConverter<FinanceTransferId, Guid> TransferId = new(id => id.Value, v => FinanceTransferId.From(v));
    public static readonly ValueConverter<BankStatementId, Guid> StatementId = new(id => id.Value, v => BankStatementId.From(v));
    public static readonly ValueConverter<BankStatementLineId, Guid> StatementLineId =
        new(id => id.Value, v => BankStatementLineId.From(v));
}
