using System.Text;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes.Identifiers;
using Crm.Infrastructure.Persistence.Documents.Artifacts;

namespace IntegrationTests.Crm.Documents;

public sealed class QuotePdfArtifactStateTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);
    private static QuotePdfArtifact Queued() => QuotePdfArtifact.Queue(new QuoteId(1), 2,
        new IssuerSettingsVersionId(3), new DocumentTemplateVersionId(4), "standard-v1", Start);

    [Test]
    public void Successful_bytes_are_hashed_and_immutable()
    {
        var artifact = Queued();
        var token = artifact.Claim(Start, TimeSpan.FromMinutes(5));
        var pdf = Encoding.ASCII.GetBytes("%PDF-1.7\n%%EOF");
        artifact.Complete(token, pdf, Start.AddSeconds(1));
        Assert.Multiple(() =>
        {
            Assert.That(artifact.Status, Is.EqualTo("succeeded"));
            Assert.That(artifact.Length, Is.EqualTo(pdf.Length));
            Assert.That(artifact.HasIntactContent(), Is.True);
        });
        pdf[0] = (byte)'x';
        Assert.That(artifact.HasIntactContent(), Is.True, "Stored bytes must not alias caller memory.");
        Assert.Throws<InvalidOperationException>(() => artifact.Retry(Start.AddSeconds(2)));
        Assert.Throws<InvalidOperationException>(() => artifact.Complete(token,
            Encoding.ASCII.GetBytes("%PDF-1.7\n%%EOF"), Start.AddSeconds(2)));
    }

    [Test]
    public void Stale_worker_cannot_complete_after_a_new_claim()
    {
        var artifact = Queued();
        var first = artifact.Claim(Start, TimeSpan.FromMinutes(5));
        var second = artifact.Claim(Start.AddMinutes(6), TimeSpan.FromMinutes(5));
        Assert.That(second, Is.Not.EqualTo(first));
        Assert.Throws<InvalidOperationException>(() => artifact.Complete(first,
            Encoding.ASCII.GetBytes("%PDF-1.7\n%%EOF"), Start.AddMinutes(6)));
        artifact.Fail(second, "render_failed", Start.AddMinutes(6));
        Assert.That(artifact.Status, Is.EqualTo("failed"));
        artifact.Retry(Start.AddMinutes(7));
        Assert.That(artifact.Status, Is.EqualTo("queued"));
    }

    [Test]
    public void Invalid_pdf_does_not_create_a_successful_artifact()
    {
        var artifact = Queued();
        var token = artifact.Claim(Start, TimeSpan.FromMinutes(5));
        Assert.Throws<ArgumentException>(() => artifact.Complete(token,
            Encoding.ASCII.GetBytes("not a PDF"), Start.AddSeconds(1)));
        Assert.That(artifact.Status, Is.EqualTo("processing"));
        Assert.That(artifact.PdfBytes, Is.Null);
    }
}
