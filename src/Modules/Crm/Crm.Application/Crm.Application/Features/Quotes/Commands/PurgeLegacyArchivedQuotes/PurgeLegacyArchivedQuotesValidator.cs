
namespace Crm.Application.Features.Quotes.Commands.PurgeLegacyArchivedQuotes;

public sealed class PurgeLegacyArchivedQuotesValidator : AbstractValidator<PurgeLegacyArchivedQuotesCommand>
{
    public PurgeLegacyArchivedQuotesValidator() => RuleFor(x => x.Actor).NotNull();
}
