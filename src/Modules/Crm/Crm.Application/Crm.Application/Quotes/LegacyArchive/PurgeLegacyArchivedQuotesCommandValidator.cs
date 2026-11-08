using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.ProductionLinks;
using Crm.Application.Quotes.Staff;

namespace Crm.Application.Quotes.LegacyArchive;

public sealed class PurgeLegacyArchivedQuotesCommandValidator : AbstractValidator<PurgeLegacyArchivedQuotesCommand>
{
    public PurgeLegacyArchivedQuotesCommandValidator() => RuleFor(x => x.Actor).NotNull();
}
