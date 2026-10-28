namespace Crm.Application.Files.Cleanup;

// Explicit maintenance operation; never schedules itself or deletes referenced history.
public interface ICustomTemporaryFileCleanup
{
    Task<int> CleanupAsync(int maximumFiles, CancellationToken cancellationToken);
}
