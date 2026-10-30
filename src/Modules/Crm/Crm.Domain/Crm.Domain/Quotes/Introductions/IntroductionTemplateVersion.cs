using BuildingBlocks.Domain.Entity;

namespace Crm.Domain.Quotes.Introductions;

public sealed class IntroductionTemplateVersion : BaseEntity<IntroductionTemplateVersionId>
{
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public IntroductionTemplateId TemplateId { get; private set; }
    public IntroductionTemplate Template { get; private set; } = null!;
    public int Number { get; private set; }
    public string Name { get; private set; } = null!;
    public IntroductionTemplatePayload Payload { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private IntroductionTemplateVersion() { }

    public static IntroductionTemplateVersion Create(IntroductionTemplateId templateId, int number,
        string name, IntroductionTemplatePayload payload)
    {
        if (templateId.Value <= 0 || number <= 0 || string.IsNullOrWhiteSpace(name) ||
            name.Trim().Length > 300 || payload is null)
            throw new IntroductionValidationException("invalid_template_version");
        payload.ToSnapshot();
        return new IntroductionTemplateVersion { TemplateId = templateId, Number = number,
            Name = name.Trim(), Payload = payload with { Selections = payload.Selections.ToArray() } };
    }
}
