using System.Security.Cryptography;
using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Statements;
using Accounting.Application;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Statements.Shared;
using Accounting.Application.Features.Statements.Shared.Reading;

namespace Accounting.Application.Features.Statements.Commands.UploadBankStatement;

/// <summary>
/// «Завантажити виписку»: reads an XLSX, XLS or CSV statement of one account into a preview with recognised lines.
/// Nothing reaches the books until «Провести виписку». The same file of the same account returns the statement
/// uploaded before.
/// </summary>
public sealed record UploadBankStatementCommand(Guid AccountId, Stream Content, string FileName, string By)
    : ICommand<Result<BankStatementResponse>>
{
    public static readonly string[] Extensions = [".xlsx", ".xls", ".csv"];

    /// <summary>The file name without any folder path a browser may send.</summary>
    public string DisplayName => FileName is null ? string.Empty : FileName[(FileName.LastIndexOfAny(['/', '\\']) + 1)..].Trim();
}
