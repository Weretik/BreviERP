namespace Content.Application.Abstractions;

/// <summary>Photos of the «Фото» library for Content. Implemented in the host over the Media module; Content keeps only the photo id.</summary>
public interface IContentMediaReader
{
    /// <summary>The photos the library still has; missing ids are left out.</summary>
    Task<IReadOnlyList<ContentMediaPhoto>> GetAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);
}

/// <param name="ThumbnailUrl">The small WebP copy for lists; null until the library has made it.</param>
public sealed record ContentMediaPhoto(int Id, string Url, string? ThumbnailUrl, int? Width, int? Height);
