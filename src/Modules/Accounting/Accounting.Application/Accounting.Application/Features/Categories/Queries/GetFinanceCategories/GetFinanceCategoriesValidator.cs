using Accounting.Application.Abstractions;
using Accounting.Domain.Ledger;
using Accounting.Application.Features.Categories.Shared;

namespace Accounting.Application.Features.Categories.Queries.GetFinanceCategories;

public sealed class GetFinanceCategoriesValidator : AbstractValidator<GetFinanceCategoriesQuery>
{
    public GetFinanceCategoriesValidator() => RuleFor(x => x.Scope).IsInEnum().When(x => x.Scope is not null);
}
