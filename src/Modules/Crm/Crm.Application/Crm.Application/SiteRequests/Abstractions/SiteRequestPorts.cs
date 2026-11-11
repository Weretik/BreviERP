using Crm.Domain.SiteRequests;

namespace Crm.Application.SiteRequests.Abstractions;

/// <summary>Persistence port of the site requests (LEAD-002); implemented by the Crm DbContext.</summary>
public interface ISiteRequestStore
{
    DbSet<SiteRequest> SiteRequests { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

/// <summary>Keeps the attached file on the CDN; implemented in the host over the Media storage.</summary>
public interface ISiteRequestFileStorage
{
    /// <returns>The public URL of the stored file.</returns>
    Task<string> UploadAsync(Stream content, string storageKey, string contentType, CancellationToken cancellationToken);
}

/// <summary>
/// Posts a new request to the Telegram groups ticked «Заявки з сайту» on «Нагадування»; implemented in the host over
/// the Reminders bot.
/// </summary>
public interface ISiteRequestNotifier
{
    /// <returns>Null when at least one group got the message; otherwise why none did.</returns>
    Task<string?> NotifyAsync(string html, CancellationToken cancellationToken);
}

/// <summary>Where the admin is, for the link in the Telegram message.</summary>
public sealed record SiteRequestLinks(string AdminUrl)
{
    public string List => $"{AdminUrl.TrimEnd('/')}/crm/site-requests";
}
