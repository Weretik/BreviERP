namespace Crm.Infrastructure.Documents.Rendering;

public sealed class ChromiumDocumentOptions
{
    public string ExecutablePath { get; init; } = string.Empty;
    public string FontPath { get; init; } = string.Empty;
    public string WorkingDirectoryRootPath { get; init; } = string.Empty;
    public string PublicWebRootPath { get; init; } = string.Empty;
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
}
