using Crm.Domain.Leads.Events;
namespace Crm.Domain.Leads;

/// <summary>
/// What a category letter of the sheet means («Довідники», LEAD-001 R-11). The sync adds every new code it meets
/// with no description; the director writes the description.
/// </summary>
public sealed class LeadCategory : DomainEventSource
{
    private LeadCategory() { }

    public LeadCategory(string code, string? description)
    {
        Code = code;
        Description = LeadLimits.Cut(description, LeadLimits.CategoryDescriptionMaxLength);
    }

    public string Code { get; private set; } = null!;
    public string? Description { get; private set; }

    /// <summary>The director writes what the letter means; «Журнал дій» shows the old and new text (ACT-001).</summary>
    public void Describe(string? description)
    {
        var before = Description;
        Description = LeadLimits.Cut(description, LeadLimits.CategoryDescriptionMaxLength);
        Raise(new LeadCategoryDescribed(Code, before, Description));
    }
}
