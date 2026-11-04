namespace Crm.Application.Embroidery.Access;

/// <summary>
/// The signed-in staff member (EMB-001, R-02). A Manager sees the calculation and the three prices; cost, rates and
/// «Налаштування» are for Director and Admin only (the Director also holds Admin).
/// </summary>
public sealed record EmbroideryActor(int UserId, bool SeesCost);
