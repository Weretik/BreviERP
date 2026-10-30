namespace Crm.Application.Quotes.Introductions;

/// <summary>Stable user intent; mutable catalog metadata and prices never enter the apply replay key.</summary>
public sealed record IntroductionQuoteApplicationIntent(
    Guid TemplatePublicId, Guid? VersionPublicId, Guid? CustomerPublicId,
    Guid? ContactPublicId, string? RecipientName)
{
    public string Kind => "introductionTemplateApplication-v1";
}
