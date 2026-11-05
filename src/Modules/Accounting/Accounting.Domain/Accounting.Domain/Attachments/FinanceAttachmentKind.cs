namespace Accounting.Domain.Attachments;

/// <summary>
/// The only files an operation takes: a PDF, a JPEG or a PNG. The type is told by the first bytes of the file, not by
/// its name or by what the browser says.
/// </summary>
public sealed record FinanceAttachmentKind(string ContentType, string Extension)
{
    public static readonly FinanceAttachmentKind Pdf = new("application/pdf", ".pdf");
    public static readonly FinanceAttachmentKind Jpeg = new("image/jpeg", ".jpg");
    public static readonly FinanceAttachmentKind Png = new("image/png", ".png");

    /// <summary>How many first bytes <see cref="Detect"/> needs.</summary>
    public const int SignatureLength = 8;

    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    /// <returns>The kind of the file by its first bytes, or null when it is none of the three.</returns>
    public static FinanceAttachmentKind? Detect(ReadOnlySpan<byte> head) =>
        head.StartsWith(PdfSignature) ? Pdf
        : head.StartsWith(JpegSignature) ? Jpeg
        : head.StartsWith(PngSignature) ? Png
        : null;

    /// <returns>The kind with this MIME type, or null.</returns>
    public static FinanceAttachmentKind? Of(string contentType) =>
        new[] { Pdf, Jpeg, Png }.FirstOrDefault(x => x.ContentType == contentType);
}
