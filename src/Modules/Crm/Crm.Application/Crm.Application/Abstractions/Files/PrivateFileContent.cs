namespace Crm.Application.Abstractions.Files;
public sealed record PrivateFileContent(PrivateStoredFile File, ReadOnlyMemory<byte> Bytes);
