namespace Crm.Domain.SiteRequests;

/// <summary>Where a request of the site is (LEAD-002).</summary>
public enum SiteRequestStatus
{
    /// <summary>Came from the site; nobody took it yet.</summary>
    New = 1,

    /// <summary>A manager took it and calls back.</summary>
    InWork = 2,

    /// <summary>The manager called back; what came of it is in the note.</summary>
    Done = 3,

    /// <summary>Not a request: an advertisement, a mistake or a test.</summary>
    Spam = 4,
}
