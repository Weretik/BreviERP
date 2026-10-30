namespace Crm.Infrastructure.Files.Upload;

public sealed class PrivateUploadOptions
{
    public TimeSpan TemporaryLifetime { get; init; } = TimeSpan.FromHours(24);
}
