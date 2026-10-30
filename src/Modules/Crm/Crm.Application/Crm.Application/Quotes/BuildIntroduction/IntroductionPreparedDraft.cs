using Crm.Domain.Quotes.Introductions;

namespace Crm.Application.Quotes.BuildIntroduction;

/// <summary>Validated current input for the existing quote-draft pipeline; it is not a saved quote.</summary>
public sealed record IntroductionPreparedDraft(
    IntroductionLayout Layout,
    Guid MappingVersionId,
    Guid EmailTemplateVersionId,
    IReadOnlyList<IntroductionPreparedSlot> Slots);
