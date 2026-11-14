using System.Globalization;
using System.Text.Json;
using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Reports.Shared;

namespace Accounting.Application.Features.Audit.Queries.GetFinanceAudit;

public sealed class GetFinanceAuditValidator : AbstractValidator<GetFinanceAuditQuery>
{
    public GetFinanceAuditValidator()
    {
        RuleFor(x => x.Entity).IsInEnum().When(x => x.Entity is not null);
        RuleFor(x => x.Action).IsInEnum().When(x => x.Action is not null);
        RuleFor(x => x.Page).GreaterThan(0);
    }
}
