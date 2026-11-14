using Accounting.Application.Abstractions;
using Accounting.Domain.Attachments;
using Accounting.Domain.Audit;
using Microsoft.Extensions.Logging;
using Accounting.Application.Features.Attachments.Shared;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Journal.Shared;

namespace Accounting.Application.Features.Attachments.Commands.DeleteFinanceAttachment;

public sealed class DeleteFinanceAttachmentValidator : AbstractValidator<DeleteFinanceAttachmentCommand>
{
    public DeleteFinanceAttachmentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.By).ValidBy();
    }
}
