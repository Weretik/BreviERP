using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Domain.Errors;

namespace Accounting.Application.Import;

/// <summary>
/// Reads the export file. The admin gzips it in the browser so it fits Cloud Run's request limit; a file that starts
/// with the gzip signature is unpacked, any other one is read as is. The unpacked file may not exceed
/// <see cref="MaxBytes"/>.
/// </summary>
internal static class LegacyFinanceReader
{
    public const string Format = "brevi-finance-v5";
    public const long MaxBytes = 200L * 1024 * 1024;

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        Converters = { new FlagConverter(), new TextConverter() },
    };

    /// <summary>
    /// The file, or the reason it cannot be read (not JSON of the expected shape, a broken archive, too large). It is
    /// read as a stream, never whole.
    /// </summary>
    public static async Task<DomainResult<LegacyFinanceFile>> ReadAsync(Stream content, CancellationToken cancellationToken)
    {
        var seekable = content.CanSeek ? content : await BufferAsync(content, cancellationToken);
        var start = seekable.Position;
        var gzipped = seekable.ReadByte() == 0x1f && seekable.ReadByte() == 0x8b;
        seekable.Position = start;
        await using var json = new LimitedStream(
            gzipped ? new GZipStream(seekable, CompressionMode.Decompress, leaveOpen: true) : seekable, MaxBytes);
        try
        {
            var file = await JsonSerializer.DeserializeAsync<LegacyFinanceFile>(json, Json, cancellationToken);
            return file is null || (file.Format is null && file.Transactions is null)
                ? FileError(ImportMessages.NotJson)
                : file;
        }
        catch (JsonException)
        {
            return FileError(ImportMessages.NotJson);
        }
        catch (InvalidDataException)
        {
            return FileError(ImportMessages.BrokenArchive);
        }
        catch (TooLargeException)
        {
            return FileError(ImportMessages.TooLarge);
        }
    }

    private static Error FileError(string message) => Error.Validation("Accounting.Import.File", message, "File");

    private static async Task<Stream> BufferAsync(Stream content, CancellationToken cancellationToken)
    {
        var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;
        return buffer;
    }

    /// <summary>Reads at most <c>limit</c> bytes of the unpacked file, so a small archive cannot unpack into gigabytes.</summary>
    private sealed class LimitedStream(Stream inner, long limit) : Stream
    {
        private long read;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => read; set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count) => Count(inner.Read(buffer, offset, count));

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            Count(await inner.ReadAsync(buffer, cancellationToken));

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

        private int Count(int bytes)
        {
            read += bytes;
            return read > limit ? throw new TooLargeException() : bytes;
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing && inner is GZipStream) inner.Dispose();
            base.Dispose(disposing);
        }
    }

    /// <summary>Stops the JSON reader from inside the stream once the unpacked file passes the limit.</summary>
    private sealed class TooLargeException : Exception;

    /// <summary>The old export writes flags as 0/1; true/false is read too.</summary>
    private sealed class FlagConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType switch
            {
                JsonTokenType.True => true,
                JsonTokenType.False or JsonTokenType.Null => false,
                JsonTokenType.Number => reader.GetInt64() != 0,
                JsonTokenType.String => reader.GetString() is "1" or "true",
                _ => throw new JsonException("Flag expected."),
            };

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
            writer.WriteBooleanValue(value);
    }

    /// <summary>Ids such as a supplier id may come as numbers; they are read as text.</summary>
    private sealed class TextConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => System.Text.Encoding.UTF8.GetString(reader.ValueSpan),
                JsonTokenType.Null => null,
                JsonTokenType.True => "true",
                JsonTokenType.False => "false",
                _ => throw new JsonException("Text expected."),
            };

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value);
    }
}
