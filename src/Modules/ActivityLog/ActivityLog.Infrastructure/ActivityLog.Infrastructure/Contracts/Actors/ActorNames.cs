using ActivityLog.Application.Abstractions;
using ActivityLog.Contracts.Actors;

namespace ActivityLog.Infrastructure.Contracts.Actors;

/// <summary>The first name a registered <see cref="IActivityActorNames"/> (Identity) knows, else null (ADR-0019).</summary>
internal sealed class ActorNames(IEnumerable<IActivityActorNames> sources) : IActorNames
{
    public async Task<string?> FindAsync(int userId, CancellationToken cancellationToken)
    {
        foreach (var source in sources)
            if (await source.FindAsync(userId, cancellationToken) is { Length: > 0 } name)
                return name;
        return null;
    }
}
