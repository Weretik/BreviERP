using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Movements;
using Accounting.Domain.Statements;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Statements.Shared;

namespace Accounting.Application.Features.Statements.Queries.GetBankStatement;

public sealed class GetBankStatementValidator : AbstractValidator<GetBankStatementQuery>
{
    public GetBankStatementValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.State).IsInEnum();
        RuleFor(x => x.Page).InclusiveBetween(1, 1_000);
    }
}
