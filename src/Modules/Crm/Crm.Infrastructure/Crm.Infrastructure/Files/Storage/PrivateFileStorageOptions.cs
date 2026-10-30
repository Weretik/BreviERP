namespace Crm.Infrastructure.Files.Storage;

public sealed class PrivateFileStorageOptions
{
    public string RootPath { get; init; } = string.Empty;
    public string PublicWebRootPath { get; init; } = string.Empty;
}
