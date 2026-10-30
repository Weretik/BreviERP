using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Ardalis.Result;
using Crm.Application.Abstractions.Files;
using Crm.Application.CustomSewing.Production.Contracts;
using Crm.Application.CustomSewing.Production.Projection;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.CustomSewing.Identifiers;
using Crm.Infrastructure.Documents.Production;
using Moq;

namespace IntegrationTests.Crm.Documents;

public sealed class ProductionPackageArchiveTests
{
    private static readonly Guid DraftId = Guid.NewGuid();
    private static readonly Guid FileId = Guid.NewGuid();
    private static readonly byte[] Pdf = Encoding.ASCII.GetBytes("%PDF-1.7\nproduction artwork\n%%EOF");
    private static string Hash => Convert.ToHexString(SHA256.HashData(Pdf));
    private static ProductionPackageFile File => new(FileId, $"files/{FileId:D}.pdf", "application/pdf", Pdf.Length, Hash);
    private static ProductionPackageManifest Manifest(ProductionPackageFile file) => new(1, "brevi.production.v1",
        DraftId, FileId, 1, "1-10-2026", "UAH", [], [file]);
    private static ProductionCustomVersionSource Source => new(new CustomVersionId(1),
        new QuoteCustomVersionSnapshot(DraftId, Guid.NewGuid(), 1, null!, null!),
        [new ProductionPinnedFile(FileId, "application/pdf", Pdf.Length, Hash, true)]);

    [Test]
    public async Task Verified_pinned_files_produce_a_deterministic_zip_with_a_manifest()
    {
        var store = new Mock<IPrivateFileStore>(MockBehavior.Strict);
        store.Setup(x => x.ReadAsync(DraftId, FileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new PrivateFileContent(new PrivateStoredFile(FileId, DraftId,
                "application/pdf", Pdf.Length, Hash), Pdf)));
        var builder = new ProductionPackageArchiveBuilder(store.Object);
        var manifest = Manifest(File);
        var first = await builder.BuildAsync(manifest, [Source], default);
        var second = await builder.BuildAsync(manifest, [Source], default);
        Assert.That(first.IsSuccess, Is.True);
        Assert.That(second.Value, Is.EqualTo(first.Value));
        using var zip = new ZipArchive(new MemoryStream(first.Value), ZipArchiveMode.Read);
        Assert.That(zip.Entries.Select(x => x.FullName), Is.EquivalentTo(new[] { "manifest.json", File.ArchivePath }));
        using var content = new MemoryStream();
        await zip.GetEntry(File.ArchivePath)!.Open().CopyToAsync(content);
        Assert.That(content.ToArray(), Is.EqualTo(Pdf));
        Assert.That(zip.Entries.All(x => x.LastWriteTime.Year == 1980), Is.True);
    }

    [Test]
    public async Task Changed_source_bytes_block_package_instead_of_exporting_unverified_artwork()
    {
        var tampered = Pdf.ToArray(); tampered[12] ^= 1;
        var store = new Mock<IPrivateFileStore>();
        store.Setup(x => x.ReadAsync(DraftId, FileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new PrivateFileContent(new PrivateStoredFile(FileId, DraftId,
                "application/pdf", Pdf.Length, Hash), tampered)));
        var result = await new ProductionPackageArchiveBuilder(store.Object).BuildAsync(Manifest(File), [Source], default);
        Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(result.ValidationErrors.Single().ErrorMessage, Is.EqualTo("production_file_integrity_failed"));
    }

    [Test]
    public async Task Archive_path_injection_is_rejected_before_reading_private_storage()
    {
        var store = new Mock<IPrivateFileStore>(MockBehavior.Strict);
        var result = await new ProductionPackageArchiveBuilder(store.Object)
            .BuildAsync(Manifest(File with { ArchivePath = "../private.pdf" }), [Source], default);
        Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
        store.VerifyNoOtherCalls();
    }
}
