using Identity.Application.Services;

namespace Identity.Infrastructure.Avatars;

/// <summary>Used when the host registers no avatar storage: uploads answer 503, no URLs are built.</summary>
internal sealed class UnconfiguredAvatarStorage : IAvatarStorage
{
    public bool IsConfigured => false;

    public Task SaveAsync(string key, byte[] content, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Avatar storage is not configured.");

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public string? BuildUrl(string? key) => null;
}
