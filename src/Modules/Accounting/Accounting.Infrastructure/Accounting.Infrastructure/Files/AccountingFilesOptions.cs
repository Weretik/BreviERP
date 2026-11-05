namespace Accounting.Infrastructure.Files;

/// <summary>
/// <c>Accounting:Files</c>: where operation files live. On Cloud Run <see cref="Bucket"/> is a private Google Cloud
/// Storage bucket reached with the runtime service account; for a local run <see cref="LocalPath"/> may be a folder.
/// With neither, uploads answer «Сховище файлів бухгалтерії ще не налаштоване».
/// </summary>
public sealed class AccountingFilesOptions
{
    public const string Section = "Accounting:Files";

    public string? Bucket { get; set; }
    public string? LocalPath { get; set; }
}
