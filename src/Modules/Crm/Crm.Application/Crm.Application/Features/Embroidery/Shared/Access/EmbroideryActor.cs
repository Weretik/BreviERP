namespace Crm.Application.Features.Embroidery.Shared.Access;

/// <summary>
/// The signed-in staff member (EMB-001, R-02). A Manager sees the calculation and the three prices; cost, rates and
/// «Налаштування» need «Ціна вишивки: собівартість і налаштування» on «Права доступу» (ACC-002 R-02).
/// </summary>
public sealed record EmbroideryActor(int UserId, bool SeesCost);
