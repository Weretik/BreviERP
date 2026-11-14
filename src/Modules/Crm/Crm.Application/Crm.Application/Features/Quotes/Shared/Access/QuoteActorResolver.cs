using Crm.Domain.Quotes;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Shared.Access;

internal static class QuoteActorResolver
{
    /// <summary>The actor's current name, so the КП shows who made or changed it.</summary>
    public static async Task<QuotePerson?> PersonAsync(this IQuoteStaffReader staff, QuoteActor actor,
        CancellationToken cancellationToken) =>
        await staff.FindAsync(actor.UserId, cancellationToken) is { } member
            ? new QuotePerson(member.UserId, member.Name)
            : null;
}
