using Crm.Application.Quotes.Pricing;
using Crm.Domain.Quotes.Introductions;

namespace Crm.Application.Quotes.BuildIntroduction;

public sealed record IntroductionCatalogOffer(
    IntroductionSeason Season,
    string Kind,
    bool Approved,
    bool Sellable,
    string ModelName,
    string FabricName,
    string FabricComposition,
    string FabricDensity,
    string ProductUrl,
    QuoteCatalogPrice CurrentPrice);
