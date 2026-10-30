using Crm.Application.Quotes.Pricing;
using Crm.Domain.Quotes.Introductions;

namespace Crm.Application.Quotes.BuildIntroduction;

public sealed record IntroductionPreparedSlot(
    IntroductionSelection Selection,
    string ModelName,
    string FabricName,
    string FabricComposition,
    string FabricDensity,
    string ProductUrl,
    decimal LargeWholesale,
    QuoteCatalogPrice CurrentPrice);
