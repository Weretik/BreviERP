namespace Host.Api.Security.AnonymousApiAccess;

public sealed record AnonymousApiActor(int UserId, string Name, string Email);

public interface IAnonymousApiActorResolver
{
    Task<AnonymousApiActor?> ResolveAsync(CancellationToken cancellationToken);
}
