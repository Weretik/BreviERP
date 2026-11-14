using System.Security.Cryptography;
using Accounting.Application.Abstractions;
using Accounting.Domain.Attachments;
using Accounting.Domain.Audit;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Microsoft.Extensions.Logging;
using Accounting.Application;
using Accounting.Application.Features.Attachments.Shared;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Journal.Shared;

namespace Accounting.Application.Features.Attachments.Commands.UploadFinanceAttachment;

public sealed class UploadFinanceAttachmentValidator : AbstractValidator<UploadFinanceAttachmentCommand>
{
    public UploadFinanceAttachmentValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.DisplayName).NotEmpty().WithMessage("Назва файлу обовʼязкова.")
            .MaximumLength(FinanceAttachment.FileNameMaxLength);
        RuleFor(x => x.By).ValidBy();
    }
}
