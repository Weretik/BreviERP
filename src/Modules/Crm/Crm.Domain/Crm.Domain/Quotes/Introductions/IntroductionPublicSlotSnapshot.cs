namespace Crm.Domain.Quotes.Introductions;

public sealed record IntroductionPublicSlotSnapshot(
    IntroductionSlot Slot,
    int ProductId,
    long ArticleId,
    string ModelName,
    string FabricName,
    string FabricComposition,
    string FabricDensity,
    string ProductUrl,
    decimal LargeWholesale);
