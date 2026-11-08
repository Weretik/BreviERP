using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.ProductionLinks;
using Crm.Application.Quotes.Staff;

namespace Crm.Application.Quotes.LegacyArchive;

public sealed class GetLegacyArchivedQuotesQueryValidator : AbstractValidator<GetLegacyArchivedQuotesQuery>
{
    public GetLegacyArchivedQuotesQueryValidator() => RuleFor(x => x.Actor).NotNull();
}
