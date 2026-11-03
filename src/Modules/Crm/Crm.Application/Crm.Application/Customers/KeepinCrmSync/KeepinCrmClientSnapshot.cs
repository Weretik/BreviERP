namespace Crm.Application.Customers.KeepinCrmSync;

/// <summary>One KeepinCRM client (GET /clients with q[lead_false]=true): leads are not clients.</summary>
/// <param name="Company">«Компанія»; empty for a private person.</param>
/// <param name="Person">Contact person; the name of a private person.</param>
/// <param name="Archived">Archived in KeepinCRM: kept in Brevi as not used.</param>
public sealed record KeepinCrmClientSnapshot(
    string KeepinCrmId,
    string? Company,
    string? Person,
    IReadOnlyList<string> Phones,
    IReadOnlyList<string> Emails,
    bool Archived);
