namespace Media.Infrastructure.Integrations.AdmTools;

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
