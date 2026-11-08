using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.Access;

/// <summary>
/// The signed-in staff member. A Manager works only with КП they are responsible for; Director and Admin
/// (the director also holds Admin) see and change all of them (R-01, R-02).
/// </summary>
/// <param name="CanDelete">«КП: видалення»: only the Director for now; the access rights page will hand it out (R-33).</param>
public sealed record QuoteActor(int UserId, bool CanSeeAll, bool CanDelete = false)
{
    public bool CanAccess(Quote quote) => CanSeeAll || quote.ResponsibleUserId == UserId;
}
