using System.IO.Compression;

namespace Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Files;

/// <summary>
/// The admin gzips big price files in the browser so three Insight lists (about 17 MB each) fit Cloud Run's request
/// limit; a file that starts with the gzip signature is unpacked, any other file is read as is.
/// </summary>
internal static class CompressedUpload
{
    public static Stream Open(Stream content)
    {
        var seekable = content.CanSeek ? content : Buffer(content);
        var start = seekable.Position;
        var first = seekable.ReadByte();
        var second = seekable.ReadByte();
        seekable.Position = start;
        return first == 0x1f && second == 0x8b ? new GZipStream(seekable, CompressionMode.Decompress) : seekable;
    }

    private static MemoryStream Buffer(Stream content)
    {
        var buffer = new MemoryStream();
        content.CopyTo(buffer);
        buffer.Position = 0;
        return buffer;
    }
}
