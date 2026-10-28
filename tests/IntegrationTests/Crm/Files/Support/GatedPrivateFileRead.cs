using Ardalis.Result;
using Crm.Application.Abstractions.Files;

namespace IntegrationTests.Crm.Files.Support;

internal sealed class GatedPrivateFileRead(IPrivateFileStore inner, Guid fileId) : IPrivateFileStore
{
    internal TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    internal TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public Task<Result<PrivateStoredFile>> WriteAsync(Guid root, Stream content, string type, CancellationToken ct)
        => inner.WriteAsync(root, content, type, ct);
    public async Task<Result<PrivateFileContent>> ReadAsync(Guid root, Guid file, CancellationToken ct)
    {
        if (file == fileId)
        {
            Entered.TrySetResult();
            await Release.Task.WaitAsync(ct);
        }
        return await inner.ReadAsync(root, file, ct);
    }
}
