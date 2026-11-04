namespace Crm.Domain.Quotes.Sending;

/// <summary>
/// The Gmail a staff member sends КП from (R-15). The Google refresh token is stored encrypted; only the send
/// permission is granted, Brevi never reads the mailbox.
/// </summary>
public sealed class StaffMailbox
{
    private StaffMailbox() { }

    public StaffMailbox(int userId, string email, string protectedRefreshToken, string connectedByName, DateTimeOffset now)
    {
        UserId = userId;
        Connect(email, protectedRefreshToken, connectedByName, now);
    }

    public int UserId { get; private set; }
    public string Email { get; private set; } = null!;
    public string ProtectedRefreshToken { get; private set; } = null!;
    public string ConnectedByName { get; private set; } = null!;
    public DateTimeOffset ConnectedAt { get; private set; }

    /// <summary>A new Google consent replaces the address and the token.</summary>
    public void Connect(string email, string protectedRefreshToken, string connectedByName, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("A mailbox needs an address.", nameof(email));
        if (string.IsNullOrWhiteSpace(protectedRefreshToken))
            throw new ArgumentException("A mailbox needs a token.", nameof(protectedRefreshToken));
        Email = email.Trim().ToLowerInvariant();
        ProtectedRefreshToken = protectedRefreshToken;
        ConnectedByName = connectedByName.Trim();
        ConnectedAt = now;
    }
}
