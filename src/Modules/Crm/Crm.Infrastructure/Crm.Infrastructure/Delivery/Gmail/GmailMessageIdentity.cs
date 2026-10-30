using System.Net.Mail;

namespace Crm.Infrastructure.Delivery.Gmail;

/// <summary>Correlation identity, not a Gmail server-side deduplication guarantee.</summary>
internal static class GmailMessageIdentity
{
    internal static string For(Guid deliveryId, string sender) => $"brevi-v1-{deliveryId:N}@{new MailAddress(sender).Host.ToLowerInvariant()}";
}
