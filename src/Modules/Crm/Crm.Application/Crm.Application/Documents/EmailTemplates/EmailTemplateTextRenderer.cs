using System.Net;
using System.Text.RegularExpressions;

namespace Crm.Application.Documents.EmailTemplates;

/// <summary>Non-recursive substitution of three explicit values; no executable template engine.</summary>
public static class EmailTemplateTextRenderer
{
    private static readonly Regex Placeholder = new(@"\{(recipientName|managerName|quoteNumber)\}", RegexOptions.CultureInvariant);
    public static RenderedEmailTemplate Render(EmailTemplateDetail template, string recipientName, string managerName, string quoteNumber)
    {
        var values = new Dictionary<string, string> { ["recipientName"] = Value(recipientName), ["managerName"] = Value(managerName), ["quoteNumber"] = Value(quoteNumber) };
        string Substitute(string text) => Placeholder.Replace(text, match => values[match.Groups[1].Value]);
        var subject = Substitute(template.Subject);
        var body = Substitute(template.Body);
        if (subject.Length > 200 || body.Length > 3000) throw new ArgumentException("email_rendered_text_too_large");
        return new RenderedEmailTemplate(subject, body, WebUtility.HtmlEncode(body).Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Replace("\n", "<br>", StringComparison.Ordinal));
    }
    private static string Value(string value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 200 && !value.Contains('\r') && !value.Contains('\n') && !value.Contains('\0')
        ? value.Trim() : throw new ArgumentException("email_template_value_invalid");
}
