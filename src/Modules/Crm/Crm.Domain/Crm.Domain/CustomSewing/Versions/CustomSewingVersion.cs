using BuildingBlocks.Domain.Entity;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Composition;
using Crm.Domain.CustomSewing.Identifiers;
using Crm.Domain.CustomSewing.Pricing;

namespace Crm.Domain.CustomSewing.Versions;

public sealed class CustomSewingVersion : BaseEntity<CustomVersionId>
{
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public CustomDraftId DraftId { get; private set; }
    public Crm.Domain.CustomSewing.Drafts.CustomSewingDraft Draft { get; private set; } = null!;
    public int Number { get; private set; }
    public string Name { get; private set; } = null!;
    public CustomVersionId? ParentVersionId { get; private set; }
    public CustomSewingVersion? ParentVersion { get; private set; }
    public CustomConfiguration Configuration { get; private set; } = null!;
    public CustomPriceBasis Basis { get; private set; } = null!;
    public CrmUserId ActorId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    private CustomSewingVersion() { }

    public static CustomSewingVersion Create(CustomDraftId draftId, int number, CustomVersionId? parent,
        CustomConfiguration configuration, CustomPriceBasis basis, CrmUserId actor)
    {
        if (draftId.Value <= 0 || number <= 0 || actor.Value <= 0 || parent is { Value: <= 0 }
            || (number == 1) != (parent is null) || !basis.IsCalculated || basis.Token == Guid.Empty)
            throw new CustomSewingValidationException("invalid_custom_version");
        return new() { DraftId = draftId, Number = number, ParentVersionId = parent,
            Name = configuration.Name.Trim(), Configuration = configuration.Freeze(), Basis = basis.Freeze(), ActorId = actor };
    }
}
