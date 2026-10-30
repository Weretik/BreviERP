using Crm.Domain.CustomSewing.Composition;

namespace Crm.Domain.CustomSewing.Templates;

// Only reusable selections and norms. No money, private notes, files or price-basis snapshots.
public sealed record TemplateSelections(string Name, string ProductType, string Season, string Description,
    string ClientComment, IReadOnlyList<CustomMaterial> Fabrics, IReadOnlyList<CustomMaterial> Accessories,
    IReadOnlyList<CustomOperation> Operations, IReadOnlyList<TemplateApplication> Applications)
{
    public TemplateSelections Freeze()
    {
        if (Applications is null || Applications.Any(x => x is null))
            throw new CustomSewingValidationException("invalid_template_applications");
        var draft = ToDraft().Freeze();
        return this with { Name = draft.Name, ProductType = draft.ProductType, Fabrics = draft.Fabrics, Accessories = draft.Accessories, Operations = draft.Operations,
            Applications = Array.AsReadOnly(Applications.ToArray()) };
    }

    public CustomConfiguration ToDraft() => new(Name, ProductType, Season, Description, ClientComment, string.Empty,
        Fabrics, Accessories, Operations,
        Applications?.Select(x => new CustomApplication(x.Kind, x.Location, null, 0, 0, false)).ToArray()!, 0, 0);
}
