using Accounting.Application.Abstractions;
using Accounting.Domain.Transactions;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.Features.Attachments.Queries.GetFinanceAttachments;

public sealed class GetFinanceAttachmentsValidator : AbstractValidator<GetFinanceAttachmentsQuery>
{
    public GetFinanceAttachmentsValidator() => RuleFor(x => x.TransactionId).NotEmpty();
}
