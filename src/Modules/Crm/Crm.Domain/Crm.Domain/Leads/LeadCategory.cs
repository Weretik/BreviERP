namespace Crm.Domain.Leads;

/// <summary>
/// What a category letter of the sheet means («Довідники», LEAD-001 R-11). The sync adds every new code it meets
/// with no description; the director writes the description.
/// </summary>
public sealed class LeadCategory
{
    private LeadCategory() { }

    public LeadCategory(string code, string? description)
    {
        Code = code;
        Describe(description);
    }

    public string Code { get; private set; } = null!;
    public string? Description { get; private set; }

    public void Describe(string? description) =>
        Description = LeadLimits.Cut(description, LeadLimits.CategoryDescriptionMaxLength);
}
