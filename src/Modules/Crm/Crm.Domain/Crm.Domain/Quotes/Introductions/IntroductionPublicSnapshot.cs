namespace Crm.Domain.Quotes.Introductions;

/// <summary>Customer-visible values frozen with a quote version, independent of later catalog changes.</summary>
public sealed record IntroductionPublicSnapshot(
    IntroductionLayout Layout,
    Guid MappingVersionId,
    Guid EmailTemplateVersionId,
    IReadOnlyList<IntroductionPublicSlotSnapshot> Slots);
