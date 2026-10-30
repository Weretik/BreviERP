using System.Diagnostics;
using Crm.Application.Abstractions.Documents;
using Crm.Infrastructure.Files.Storage;

namespace Crm.Infrastructure.Documents.Rendering;

public sealed class ChromiumPrivateDocumentRenderer : IPrivateDocumentRenderer
{
    private readonly ChromiumDocumentOptions options;
    private readonly string root;
    private readonly SemaphoreSlim concurrency = new(1, 1);

    public ChromiumPrivateDocumentRenderer(ChromiumDocumentOptions options)
    {
        if (!Path.IsPathFullyQualified(options.ExecutablePath) || !File.Exists(options.ExecutablePath) || !Path.IsPathFullyQualified(options.FontPath) || !File.Exists(options.FontPath) || new FileInfo(options.FontPath).Length > 10 * 1024 * 1024 || options.Timeout < TimeSpan.FromSeconds(1) || options.Timeout > TimeSpan.FromMinutes(2))
            throw new InvalidOperationException("A provisioned Chromium executable, licensed TTF font and bounded timeout must be configured.");
        this.options = options;
        root = PrivateStorageDirectory.Open(new PrivateFileStorageOptions { RootPath = options.WorkingDirectoryRootPath, PublicWebRootPath = options.PublicWebRootPath });
    }

    public async Task<byte[]> RenderAsync(PublicDocumentProjection document, CancellationToken cancellationToken)
    {
        await concurrency.WaitAsync(cancellationToken);
        var workspace = Path.Combine(root, Guid.NewGuid().ToString("N"));
        try
        {
            var html = PublicDocumentHtmlWriter.Write(document, await File.ReadAllBytesAsync(options.FontPath, cancellationToken));
            PrivateStorageDirectory.EnsurePrivate(root);
            Directory.CreateDirectory(workspace);
            var htmlPath = Path.Combine(workspace, "document.html");
            var pdfPath = Path.Combine(workspace, "document.pdf");
            await File.WriteAllTextAsync(htmlPath, html, cancellationToken);
            var start = new ProcessStartInfo(options.ExecutablePath) { UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = workspace };
            foreach (var argument in new[] { "--headless", "--disable-background-networking", "--disable-component-update", "--disable-extensions", "--disable-sync", "--disable-default-apps", "--no-first-run", "--no-default-browser-check", "--metrics-recording-only", "--no-pdf-header-footer", "--host-resolver-rules=MAP * ~NOTFOUND", "--user-data-dir=" + Path.Combine(workspace, "profile"), "--print-to-pdf=" + pdfPath, new Uri(htmlPath).AbsoluteUri }) start.ArgumentList.Add(argument);
            using var process = Process.Start(start) ?? throw new InvalidOperationException("Cannot start the configured Chromium renderer.");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(options.Timeout);
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException)
            {
                if (!process.HasExited) process.Kill(true);
                await process.WaitForExitAsync(CancellationToken.None);
                cancellationToken.ThrowIfCancellationRequested();
                throw new TimeoutException("Document renderer exceeded its configured timeout.");
            }
            if (process.ExitCode != 0 || !File.Exists(pdfPath) || new FileInfo(pdfPath).Length > 64 * 1024 * 1024)
                throw new InvalidOperationException($"Configured renderer did not produce a bounded PDF (exit code {process.ExitCode}).");
            var bytes = await File.ReadAllBytesAsync(pdfPath, cancellationToken);
            if (!bytes.AsSpan().StartsWith("%PDF-"u8)) throw new InvalidOperationException("Configured renderer returned invalid PDF bytes.");
            return bytes;
        }
        finally
        {
            try { if (Directory.Exists(workspace)) Directory.Delete(workspace, true); }
            finally { concurrency.Release(); }
        }
    }
}
