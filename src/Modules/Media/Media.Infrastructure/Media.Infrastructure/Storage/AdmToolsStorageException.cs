namespace Media.Infrastructure.Storage;

public sealed class AdmToolsStorageException : Exception
{
    public AdmToolsStorageException(string message)
        : base(message)
    {
    }

    public AdmToolsStorageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
