using System.Text;
using Crm.Application.Features.Quotes.Shared.Sending.Gmail;
using Crm.Application.Abstractions.Quotes;
using Crm.Application.Abstractions.Quotes.Sending;

namespace Crm.Infrastructure.Integrations.Gmail;

/// <summary>Builds the RFC 5322 letter with a plain text body and one attachment, UTF-8 everywhere.</summary>
internal static class GmailMimeMessage
{
    public static byte[] Build(GmailLetter letter, string boundary)
    {
        var builder = new StringBuilder();
        builder.Append("From: ").Append(EncodeWord(letter.FromName)).Append(" <").Append(letter.FromEmail).Append(">\r\n");
        builder.Append("To: ").Append(string.Join(", ", letter.To)).Append("\r\n");
        builder.Append("Subject: ").Append(EncodeWord(letter.Subject)).Append("\r\n");
        builder.Append("MIME-Version: 1.0\r\n");
        builder.Append("Content-Type: multipart/mixed; boundary=\"").Append(boundary).Append("\"\r\n\r\n");

        builder.Append("--").Append(boundary).Append("\r\n");
        builder.Append("Content-Type: text/plain; charset=UTF-8\r\n");
        builder.Append("Content-Transfer-Encoding: base64\r\n\r\n");
        AppendBase64(builder, Encoding.UTF8.GetBytes(letter.Body.ReplaceLineEndings("\r\n")));

        var attachment = letter.Attachment;
        builder.Append("--").Append(boundary).Append("\r\n");
        builder.Append("Content-Type: ").Append(attachment.ContentType).Append("; name=\"")
            .Append(EncodeWord(attachment.FileName)).Append("\"\r\n");
        builder.Append("Content-Disposition: attachment; filename*=UTF-8''")
            .Append(Uri.EscapeDataString(attachment.FileName)).Append("\r\n");
        builder.Append("Content-Transfer-Encoding: base64\r\n\r\n");
        AppendBase64(builder, attachment.Content);
        builder.Append("--").Append(boundary).Append("--\r\n");
        return Encoding.ASCII.GetBytes(builder.ToString());
    }

    /// <summary>RFC 2047 encoded words of at most 45 UTF-8 bytes each, folded onto new header lines.</summary>
    internal static string EncodeWord(string text)
    {
        if (text.All(c => c is >= ' ' and < (char)127 and not '"' and not '=' and not '?')) return text;
        var words = new List<string>();
        var chunk = new StringBuilder();
        foreach (var rune in text.EnumerateRunes())
        {
            if (Encoding.UTF8.GetByteCount(chunk.ToString()) + rune.Utf8SequenceLength > 45)
            {
                words.Add(Word(chunk.ToString()));
                chunk.Clear();
            }
            chunk.Append(rune.ToString());
        }
        if (chunk.Length > 0) words.Add(Word(chunk.ToString()));
        return string.Join("\r\n ", words);

        static string Word(string part) => $"=?UTF-8?B?{Convert.ToBase64String(Encoding.UTF8.GetBytes(part))}?=";
    }

    private static void AppendBase64(StringBuilder builder, byte[] content)
    {
        var base64 = Convert.ToBase64String(content);
        for (var index = 0; index < base64.Length; index += 76)
            builder.Append(base64, index, Math.Min(76, base64.Length - index)).Append("\r\n");
    }
}
