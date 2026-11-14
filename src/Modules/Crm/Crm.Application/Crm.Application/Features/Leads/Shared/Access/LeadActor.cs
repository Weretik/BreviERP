namespace Crm.Application.Features.Leads.Shared.Access;

/// <summary>
/// The signed-in staff member on «Ліди». Everyone with the page right sees all leads and takes free ones; only the
/// owner records calls. Director and Admin (the director also holds Admin) may also give back anybody's lead
/// (LEAD-001 R-04). A role made on «Права доступу» is treated like a manager.
/// </summary>
public sealed record LeadActor(int UserId, bool IsAdmin);
