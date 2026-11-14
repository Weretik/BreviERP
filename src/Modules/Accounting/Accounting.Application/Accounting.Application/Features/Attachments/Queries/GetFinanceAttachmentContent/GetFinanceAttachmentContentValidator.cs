using Accounting.Application.Abstractions;
using Accounting.Domain.Transactions;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.Features.Attachments.Queries.GetFinanceAttachmentContent;

public sealed class GetFinanceAttachmentContentValidator : AbstractValidator<GetFinanceAttachmentContentQuery>
{
    public GetFinanceAttachmentContentValidator() => RuleFor(x => x.Id).NotEmpty();
}
