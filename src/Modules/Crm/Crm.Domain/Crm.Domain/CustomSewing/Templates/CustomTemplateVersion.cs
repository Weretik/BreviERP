using BuildingBlocks.Domain.Entity;
using Crm.Domain.CustomSewing.Identifiers;

namespace Crm.Domain.CustomSewing.Templates;

public sealed class CustomTemplateVersion : BaseEntity<CustomTemplateVersionId>
{
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public CustomTemplateId TemplateId { get; private set; }
    public CustomTemplate Template { get; private set; } = null!;
    public int Number { get; private set; }
    public string Introduction { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string ProductType { get; private set; } = null!;
    public string Season { get; private set; } = null!;
    public TemplateSelections Selections { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    private CustomTemplateVersion() { }
    public static CustomTemplateVersion Create(CustomTemplateId templateId, int number, string introduction, TemplateSelections selections)
    {
        if (templateId.Value <= 0 || number <= 0 || introduction is null || introduction.Length > 4000 || selections is null)
            throw new CustomSewingValidationException("invalid_template_version");
        var frozen = selections.Freeze();
        return new CustomTemplateVersion { TemplateId = templateId, Number = number,
            Introduction = introduction, Selections = frozen, Name = frozen.Name, ProductType = frozen.ProductType, Season = frozen.Season };
    }
}
