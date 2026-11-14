using System.Security.Cryptography;
using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Statements;
using Accounting.Application;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Statements.Shared;
using Accounting.Application.Features.Statements.Shared.Reading;

namespace Accounting.Application.Features.Statements.Commands.UploadBankStatement;

public sealed class UploadBankStatementValidator : AbstractValidator<UploadBankStatementCommand>
{
    public UploadBankStatementValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.DisplayName).NotEmpty().WithMessage("Назва файлу обовʼязкова.")
            .Must(x => UploadBankStatementCommand.Extensions.Any(e => x.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
            .WithMessage(StatementMessages.WrongType);
        RuleFor(x => x.By).ValidBy();
    }
}
