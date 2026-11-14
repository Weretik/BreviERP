using Crm.Domain.Quotes;

namespace Crm.Application.Features.Quotes.Shared.Access;

/// <summary>
/// The signed-in staff member. A Manager works only with КП they are responsible for; Director and Admin
/// (the director also holds Admin) see and change all of them (R-01, R-02).
/// </summary>
/// <param name="CanDelete">«КП: видалення»: the «Видаляти» right of «Права доступу», checked by the API (R-33).</param>
public sealed record QuoteActor(int UserId, bool CanSeeAll, bool CanDelete = false)
{
    public bool CanAccess(Quote quote) => CanSeeAll || quote.ResponsibleUserId == UserId;
}
