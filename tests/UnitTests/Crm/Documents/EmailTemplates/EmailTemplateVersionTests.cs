using Crm.Domain.Documents.EmailTemplates;
using Crm.Domain.Customers.Identifiers;
using Crm.Application.Documents.EmailTemplates;

namespace UnitTests.Crm.Documents.EmailTemplates;
public sealed class EmailTemplateVersionTests
{
    [TestCase("Hello {unknown}", "Body")]
    [TestCase("Hello {recipientName", "Body")]
    [TestCase("Hello", "Body }")]
    [TestCase("Hello\r\nBcc: other@example.invalid", "Body")]
    public void Rejects_unsupported_placeholders_or_subject_headers(string subject, string body)
        => Assert.Throws<EmailTemplateValidationException>(() => EmailTemplateVersion.Create(1, "Template", subject, body, new CrmUserId(7)));

    [Test]
    public void Rendering_is_nonrecursive_and_escapes_customer_text_for_html()
    {
        var v = EmailTemplateVersion.Create(1, "Test", "КП {quoteNumber}", "Вітаємо {recipientName}, {managerName}", new CrmUserId(7));
        var detail = new EmailTemplateDetail(v.PublicId, v.Number, v.Name, v.Subject, v.Body, v.CreatedAt);
        var rendered = EmailTemplateTextRenderer.Render(detail, "<script>{managerName}</script>", "Олена", "10/10/2026");
        Assert.That(rendered.Subject, Is.EqualTo("КП 10/10/2026"));
        Assert.That(rendered.Body, Does.Contain("<script>{managerName}</script>"));
        Assert.That(rendered.HtmlBody, Does.Not.Contain("<script>"));
        Assert.That(rendered.HtmlBody, Does.Contain("&lt;script&gt;{managerName}&lt;/script&gt;"));
    }

    [Test]
    public void Placeholder_expansion_cannot_bypass_the_final_subject_limit()
    {
        var v = EmailTemplateVersion.Create(1, "Test", "{recipientName} {managerName}", "Body", new CrmUserId(7));
        var detail = new EmailTemplateDetail(v.PublicId, v.Number, v.Name, v.Subject, v.Body, v.CreatedAt);
        Assert.Throws<ArgumentException>(() => EmailTemplateTextRenderer.Render(detail,
            new string('a', 120), new string('b', 120), "10/10/2026"));
    }
    [Test]
    public void Activation_has_revision_and_no_fabricated_default()
    {
        var state = EmailTemplateState.Create();
        Assert.That(state.ActiveVersionId, Is.Null);
        state.Activate(1, new EmailTemplateVersionId(5));
        Assert.That(state.Revision, Is.EqualTo(2));
        Assert.Throws<EmailTemplateValidationException>(() => state.Activate(1, new EmailTemplateVersionId(6)));
    }
}
