using Crm.Application.Features.Embroidery.Shared;

namespace Crm.Application.Abstractions.Embroidery;

/// <summary>Photos of the Media library for «Макет»; implemented in the host over the Media file queries.</summary>
public interface IEmbroideryMediaReader
{
    /// <summary>The files that exist; a missing id is left out.</summary>
    Task<IReadOnlyDictionary<int, EmbroideryMockupResponse>> ReadAsync(IReadOnlyCollection<int> mediaFileIds,
        CancellationToken cancellationToken);
}

internal static class EmbroideryMockups
{
    public static async Task<IReadOnlyDictionary<int, EmbroideryMockupResponse>> ReadForAsync(
        this IEmbroideryMediaReader media, IEnumerable<int?> mediaFileIds, CancellationToken cancellationToken)
    {
        var ids = mediaFileIds.OfType<int>().Distinct().ToArray();
        return ids.Length == 0
            ? new Dictionary<int, EmbroideryMockupResponse>()
            : await media.ReadAsync(ids, cancellationToken);
    }

    public static EmbroideryMockupResponse? Of(this IReadOnlyDictionary<int, EmbroideryMockupResponse> mockups,
        int? mediaFileId) => mediaFileId is { } id ? mockups.GetValueOrDefault(id) : null;
}
