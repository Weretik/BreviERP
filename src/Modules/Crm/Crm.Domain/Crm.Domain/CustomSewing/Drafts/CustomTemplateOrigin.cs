using Crm.Domain.CustomSewing.Composition;

namespace Crm.Domain.CustomSewing.Drafts;

/// <summary>Immutable provenance retained when a template creates an independent draft.</summary>
public sealed record CustomTemplateOrigin(Guid TemplatePublicId, Guid VersionPublicId, string Introduction)
{
    public CustomTemplateOrigin Validate()
    {
        if (TemplatePublicId == Guid.Empty || VersionPublicId == Guid.Empty || Introduction is null
            || Introduction.Length > 4000)
            throw new CustomSewingValidationException("invalid_template_origin");
        return this;
    }
}
