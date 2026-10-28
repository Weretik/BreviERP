using Crm.Application.Abstractions.Documents;
using Crm.Infrastructure.Documents.Rendering;

namespace IntegrationTests.Crm.Documents;

public class ChromiumDocumentRendererTests
{
    [Test]
    public async Task Cancelled_queued_render_does_not_prepare_content_before_the_active_render_finishes()
    {
        var executable = Environment.GetEnvironmentVariable("BREVIERP_CHROMIUM_TEST_EXECUTABLE");
        var font = Environment.GetEnvironmentVariable("BREVIERP_CHROMIUM_TEST_FONT");
        if (string.IsNullOrWhiteSpace(executable) || string.IsNullOrWhiteSpace(font))
            Assert.Ignore("Set the configured renderer executable/font to verify resource admission.");
        var workspace = Path.Combine(Path.GetTempPath(), "brevi-en003-admission-" + Guid.NewGuid().ToString("N"));
        using var release = new ManualResetEventSlim();
        using var activeCancellation = new CancellationTokenSource();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<byte[]>? active = null;
        try
        {
            var renderer = new ChromiumPrivateDocumentRenderer(new ChromiumDocumentOptions
            {
                ExecutablePath = executable!, FontPath = font!,
                WorkingDirectoryRootPath = Path.Combine(workspace, "private"),
                PublicWebRootPath = Path.Combine(workspace, "wwwroot")
            });
            var activeContent = new ObservedParagraphs(() =>
            {
                entered.TrySetResult();
                if (!release.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Test content was not released.");
            });
            active = Task.Run(() => renderer.RenderAsync(new("Active", activeContent, []), activeCancellation.Token));
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            using var queuedCancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            var queuedContent = new ObservedParagraphs(() => throw new InvalidOperationException("Queued content was prepared before admission."));
            Assert.CatchAsync<OperationCanceledException>(async () =>
                await renderer.RenderAsync(new("Queued", queuedContent, []), queuedCancellation.Token));
        }
        finally
        {
            activeCancellation.Cancel();
            release.Set();
            if (active is not null)
            {
                try { await active; }
                catch (OperationCanceledException) { }
            }
            if (Directory.Exists(workspace)) Directory.Delete(workspace, true);
        }
    }

    private sealed class ObservedParagraphs(Action observe) : IReadOnlyList<string>
    {
        public int Count => 1;
        public string this[int index] => "Synthetic content";
        public IEnumerator<string> GetEnumerator()
        {
            observe();
            return ((IEnumerable<string>)new[] { "Synthetic content" }).GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Test]
    public async Task Ukrainian_multipage_projection_with_local_font_image_and_escaped_text_produces_pdf()
    {
        var executable = Environment.GetEnvironmentVariable("BREVIERP_CHROMIUM_TEST_EXECUTABLE");
        var font = Environment.GetEnvironmentVariable("BREVIERP_CHROMIUM_TEST_FONT");
        if (string.IsNullOrWhiteSpace(executable) || string.IsNullOrWhiteSpace(font))
            Assert.Ignore("Set BREVIERP_CHROMIUM_TEST_EXECUTABLE and BREVIERP_CHROMIUM_TEST_FONT to execute the provisioned-runtime adapter proof.");
        var workspace = Path.Combine(Path.GetTempPath(), "brevi-en003-render-" + Guid.NewGuid().ToString("N"));
        try
        {
            var renderer = new ChromiumPrivateDocumentRenderer(new ChromiumDocumentOptions { ExecutablePath = executable!, FontPath = font!, WorkingDirectoryRootPath = Path.Combine(workspace, "private"), PublicWebRootPath = Path.Combine(workspace, "wwwroot") });
            var paragraphs = Enumerable.Range(1, 90).Select(index => $"Рядок {index}: Український текст — Ґанок, Єдність, Історія, Їжак. Ціна та кількість зафіксовані у версії.").ToList();
            paragraphs.Insert(0, "<script>fetch('https://example.invalid/private')</script> & <img src=file:///private-secret>");
            var image = new PublicDocumentImage("Перевірене локальне зображення", "image/png", Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAKAAAAA8CAIAAABuCSZCAAAA00lEQVR4nO3dsQkCQQBFwT2xB1uxJXsQRLCHa8jAVozN1QYUTOS4x0y6Gyw8frzT63EddG2WfgD/JXCcwHECxwkcJ3CcwHHbbwe7+TxW7n44/X55f3yOlbtdPszVguMEjhM4TuA4geMEjhM4TuA4geMEjhM4TuA4geMEjhM4TuA4geMEjhM4TuA4geMEjhM4TuA4geMEjhM4TuA4geMEjhM4TuA4geMEjhM4TuA4geMEjhM4TuA4geMEjhM4TuC4yaccbRYcJ3CcwHECxwkcJ/BoewNxCwvlf33N9wAAAABJRU5ErkJggg=="));
            var pdf = await renderer.RenderAsync(new PublicDocumentProjection("Перевірка українського документа", paragraphs, new[] { image }), default);
            Assert.That(pdf.AsSpan().StartsWith("%PDF-"u8), Is.True);
            Assert.That(Directory.GetDirectories(Path.Combine(workspace, "private")), Is.Empty, "Fresh browser profile and transient source must be removed.");
            Assert.ThrowsAsync<ArgumentException>(async () => await renderer.RenderAsync(new PublicDocumentProjection("Null image", Array.Empty<string>(), new PublicDocumentImage[] { null! }), default));
            var output = Environment.GetEnvironmentVariable("BREVIERP_CHROMIUM_PROOF_OUTPUT");
            if (!string.IsNullOrEmpty(output)) await File.WriteAllBytesAsync(output, pdf);
            Assert.ThrowsAsync<ArgumentException>(async () => await renderer.RenderAsync(new PublicDocumentProjection("Invalid", Array.Empty<string>(), new[] { new PublicDocumentImage("Invalid", "image/png", "<html>"u8.ToArray()) }), default));
        }
        finally { if (Directory.Exists(workspace)) Directory.Delete(workspace, true); }
    }

    [Test]
    public void Missing_renderer_runtime_fails_closed() => Assert.Throws<InvalidOperationException>(() => new ChromiumPrivateDocumentRenderer(new ChromiumDocumentOptions()));
}
