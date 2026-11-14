using System.Security.Cryptography;
using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Statements;
using Accounting.Application;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Statements.Shared;
using Accounting.Application.Features.Statements.Shared.Reading;
using Accounting.Domain.Statements.ValueObjects;

namespace Accounting.Application.Features.Statements.Commands.UploadBankStatement;

public sealed class UploadBankStatementHandler(IAccountingDbContext db, IBankStatementFileReader reader,
    IAccountingCustomerReader customers, IAccountingSupplierReader suppliers, TimeProvider clock)
    : ICommandHandler<UploadBankStatementCommand, Result<BankStatementResponse>>
{
    public async ValueTask<Result<BankStatementResponse>> Handle(UploadBankStatementCommand command,
        CancellationToken cancellationToken)
    {
        var accountId = FinanceAccountId.From(command.AccountId);
        var account = await db.Accounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == accountId, cancellationToken);
        if (account is null) return Result.NotFound(AccountingMessages.AccountNotFound);

        using var buffer = new MemoryStream();
        await CopyAtMostAsync(command.Content, buffer, BankStatement.MaxBytes + 1, cancellationToken);
        if (buffer.Length == 0) return Invalid(StatementMessages.Empty);
        if (buffer.Length > BankStatement.MaxBytes) return Invalid(StatementMessages.TooLarge);
        var hash = Convert.ToHexStringLower(SHA256.HashData(buffer.GetBuffer().AsSpan(0, (int)buffer.Length)));

        var earlier = await FindAsync(accountId, hash, cancellationToken);
        if (earlier is not null) return Result.Success(BankStatementResponse.From(earlier, account.Name, existing: true));

        IReadOnlyList<BankSheet> sheets;
        try
        {
            buffer.Position = 0;
            sheets = await reader.ReadAsync(buffer, command.DisplayName, cancellationToken);
        }
        catch (BankStatementFileException error)
        {
            // The reader cannot open the file (format, password, size); its text is for the user.
            return Invalid(error.Message);
        }

        var parsed = BankStatementParser.Parse(sheets, account, command.DisplayName);
        if (parsed.IsFailure) return parsed.Error!.ToResult<BankStatementResponse>();
        var rows = parsed.Value;

        var created = BankStatement.Create(BankStatementId.New(), account, command.DisplayName, hash, rows,
            command.By, clock.GetUtcNow());
        if (created.IsFailure) return created.Error!.ToResult<BankStatementResponse>();
        var (statement, lines) = created.Value;
        await BankLineRecognition.RecogniseAsync(db, customers, suppliers, account, lines, cancellationToken);
        db.BankStatements.Add(statement);
        db.BankStatementLines.AddRange(lines);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // The same file was uploaded at the same moment; show that statement.
            db.ClearTracked();
            earlier = await FindAsync(accountId, hash, cancellationToken);
            if (earlier is null) throw;
            return Result.Success(BankStatementResponse.From(earlier, account.Name, existing: true));
        }

        return Result.Success(BankStatementResponse.From(statement, account.Name));
    }

    private Task<BankStatement?> FindAsync(FinanceAccountId accountId, string hash, CancellationToken cancellationToken) =>
        db.BankStatements.AsNoTracking().FirstOrDefaultAsync(x => x.AccountId == accountId && x.FileHash == hash,
            cancellationToken);

    private static Result<BankStatementResponse> Invalid(string message) =>
        Result.Invalid(new ValidationError("File", message));

    private static async Task CopyAtMostAsync(Stream source, Stream target, long limit, CancellationToken cancellationToken)
    {
        var chunk = new byte[81920];
        long total = 0;
        int read;
        while (total < limit &&
               (read = await source.ReadAsync(chunk.AsMemory(0, (int)Math.Min(chunk.Length, limit - total)),
                   cancellationToken)) > 0)
        {
            await target.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
            total += read;
        }
    }
}
