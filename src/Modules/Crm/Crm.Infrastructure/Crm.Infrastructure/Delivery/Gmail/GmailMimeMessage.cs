using System.Globalization;
using System.Text;
using Crm.Application.Delivery.Provider;

namespace Crm.Infrastructure.Delivery.Gmail;

internal static class GmailMimeMessage
{
    internal static string Encode(QuoteDeliverySubmission input, string sender)
    {
        if (input.DeliveryId == Guid.Empty || input.RequestedAt == default ||
            string.IsNullOrWhiteSpace(input.Subject) || input.Subject.Length > 200 ||
            input.Subject.Contains('\r') || input.Subject.Contains('\n') ||
            string.IsNullOrWhiteSpace(input.PlainText) || input.PlainText.Length > 3000 ||
            input.PdfBytes.Length is < 5 or > 20 * 1024 * 1024 ||
            !input.PdfBytes.Span.StartsWith("%PDF-"u8) ||
            sender.Any(c => c > 127) || input.RecipientAddress.Any(c => c > 127))
            throw new ArgumentException("gmail_message_invalid");

        var boundary = $"brevi-{input.DeliveryId:N}";
        var message = new StringBuilder()
            .Append("From: ").Append(sender).Append("\r\nTo: ").Append(input.RecipientAddress)
            .Append("\r\nDate: ").Append(input.RequestedAt.UtcDateTime.ToString("r", CultureInfo.InvariantCulture))
            .Append("\r\nMessage-ID: <").Append(GmailMessageIdentity.For(input.DeliveryId, sender)).Append(">")
            .Append("\r\nSubject: ").Append(Subject(input.Subject))
            .Append("\r\nMIME-Version: 1.0\r\nContent-Type: multipart/mixed; boundary=")
            .Append(boundary).Append("\r\n\r\n--").Append(boundary)
            .Append("\r\nContent-Type: text/plain; charset=utf-8\r\nContent-Transfer-Encoding: base64\r\n\r\n")
            .Append(Fold(Convert.ToBase64String(Encoding.UTF8.GetBytes(input.PlainText))))
            .Append("\r\n--").Append(boundary)
            .Append("\r\nContent-Type: application/pdf; name=quote.pdf\r\nContent-Disposition: attachment; filename=quote.pdf\r\nContent-Transfer-Encoding: base64\r\n\r\n")
            .Append(Fold(Convert.ToBase64String(input.PdfBytes.Span)))
            .Append("\r\n--").Append(boundary).Append("--\r\n").ToString();
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(message)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string Fold(string value) => string.Join("\r\n",
        Enumerable.Range(0, (value.Length + 75) / 76).Select(index => value.Substring(index * 76, Math.Min(76, value.Length - index * 76))));

    private static string Subject(string value)
    {
        var chunks = new List<string>();
        var buffer = new StringBuilder();
        var count = 0;
        foreach (var rune in value.EnumerateRunes())
        {
            if (count + rune.Utf8SequenceLength > 42)
            {
                chunks.Add("=?utf-8?B?" + Convert.ToBase64String(Encoding.UTF8.GetBytes(buffer.ToString())) + "?=");
                buffer.Clear(); count = 0;
            }
            buffer.Append(rune.ToString()); count += rune.Utf8SequenceLength;
        }
        if (buffer.Length > 0) chunks.Add("=?utf-8?B?" + Convert.ToBase64String(Encoding.UTF8.GetBytes(buffer.ToString())) + "?=");
        return string.Join("\r\n ", chunks);
    }
}
