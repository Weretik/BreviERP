using Crm.Domain.Embroidery.Saved;
using Crm.Domain.Embroidery.Settings;
using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Embroidery;
using Crm.Application.Features.Quotes.Shared.Mapping;
using Crm.Application.Features.Quotes.Shared.Persistence;

namespace Crm.Application.Features.Quotes.Commands.AddEmbroideryToQuote;

public sealed class AddEmbroideryToQuoteValidator : AbstractValidator<AddEmbroideryToQuoteCommand>
{
    public AddEmbroideryToQuoteValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.QuoteId).GreaterThan(0);
        RuleFor(x => x.CalculationId).GreaterThan(0);
    }
}
