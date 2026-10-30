namespace Crm.Domain.Quotes.Introductions;

/// <summary>Price-free, immutable selection and the featured mapping that validated it.</summary>
public sealed record IntroductionTemplatePayload(
    FeaturedModelMapping Mapping,
    IntroductionLayout Layout,
    Guid EmailTemplateVersionId,
    IReadOnlyList<IntroductionSelection> Selections)
{
    public IntroductionSelectionSnapshot ToSnapshot() =>
        IntroductionSelectionSnapshot.Create(Layout, Mapping, EmailTemplateVersionId, Selections);
}
