using System.Security.Cryptography;
using System.Security.AccessControl;
using System.Security.Principal;
using Ardalis.Result;
using Crm.Application.Abstractions.Files;
using Crm.Application.Customers.Access;
using Crm.Application.Files.Access;
using Crm.Application.Files.Validation;
using Crm.Infrastructure.Files.Storage;
using Moq;

namespace IntegrationTests.Crm.Files;

public class PrivateFileStoreTests
{
    private string workspace = null!;
    private string privateRoot = null!;
    private FileSystemPrivateFileStore store = null!;
    private readonly Guid scope = Guid.NewGuid();
    private static readonly byte[] Pdf = Convert.FromBase64String("JVBERi0xLjMKJZOMi54gUmVwb3J0TGFiIEdlbmVyYXRlZCBQREYgZG9jdW1lbnQgKG9wZW5zb3VyY2UpCjEgMCBvYmoKPDwKL0YxIDIgMCBSCj4+CmVuZG9iagoyIDAgb2JqCjw8Ci9CYXNlRm9udCAvSGVsdmV0aWNhIC9FbmNvZGluZyAvV2luQW5zaUVuY29kaW5nIC9OYW1lIC9GMSAvU3VidHlwZSAvVHlwZTEgL1R5cGUgL0ZvbnQKPj4KZW5kb2JqCjMgMCBvYmoKPDwKL0NvbnRlbnRzIDcgMCBSIC9NZWRpYUJveCBbIDAgMCA1OTUuMjc1NiA4NDEuODg5OCBdIC9QYXJlbnQgNiAwIFIgL1Jlc291cmNlcyA8PAovRm9udCAxIDAgUiAvUHJvY1NldCBbIC9QREYgL1RleHQgL0ltYWdlQiAvSW1hZ2VDIC9JbWFnZUkgXQo+PiAvUm90YXRlIDAgL1RyYW5zIDw8Cgo+PiAKICAvVHlwZSAvUGFnZQo+PgplbmRvYmoKNCAwIG9iago8PAovUGFnZU1vZGUgL1VzZU5vbmUgL1BhZ2VzIDYgMCBSIC9UeXBlIC9DYXRhbG9nCj4+CmVuZG9iago1IDAgb2JqCjw8Ci9BdXRob3IgKGFub255bW91cykgL0NyZWF0aW9uRGF0ZSAoRDoyMDAwMDEwMTAwMDAwMCswMCcwMCcpIC9DcmVhdG9yIChhbm9ueW1vdXMpIC9LZXl3b3JkcyAoKSAvTW9kRGF0ZSAoRDoyMDAwMDEwMTAwMDAwMCswMCcwMCcpIC9Qcm9kdWNlciAoUmVwb3J0TGFiIFBERiBMaWJyYXJ5IC0gXChvcGVuc291cmNlXCkpIAogIC9TdWJqZWN0ICh1bnNwZWNpZmllZCkgL1RpdGxlICh1bnRpdGxlZCkgL1RyYXBwZWQgL0ZhbHNlCj4+CmVuZG9iago2IDAgb2JqCjw8Ci9Db3VudCAxIC9LaWRzIFsgMyAwIFIgXSAvVHlwZSAvUGFnZXMKPj4KZW5kb2JqCjcgMCBvYmoKPDwKL0xlbmd0aCAxMTAKPj4Kc3RyZWFtCjEgMCAwIDEgMCAwIGNtICBCVCAvRjEgMTIgVGYgMTQuNCBUTCBFVApCVCAxIDAgMCAxIDcyIDc0MCBUbSAoRU4tMDAzIHJlYWwgcHJpdmF0ZSBieXRlIHJvdW5kLXRyaXApIFRqIFQqIEVUCiAKZW5kc3RyZWFtCmVuZG9iagp4cmVmCjAgOAowMDAwMDAwMDAwIDY1NTM1IGYgCjAwMDAwMDAwNjEgMDAwMDAgbiAKMDAwMDAwMDA5MiAwMDAwMCBuIAowMDAwMDAwMTk5IDAwMDAwIG4gCjAwMDAwMDA0MDIgMDAwMDAgbiAKMDAwMDAwMDQ3MCAwMDAwMCBuIAowMDAwMDAwNzMxIDAwMDAwIG4gCjAwMDAwMDA3OTAgMDAwMDAgbiAKdHJhaWxlcgo8PAovSUQgCls8MWMxNzgxOThmYmRmYTUxYjI1OTk1ZDg5ZDQxMDIwNDM+PDFjMTc4MTk4ZmJkZmE1MWIyNTk5NWQ4OWQ0MTAyMDQzPl0KJSBSZXBvcnRMYWIgZ2VuZXJhdGVkIFBERiBkb2N1bWVudCAtLSBkaWdlc3QgKG9wZW5zb3VyY2UpCgovSW5mbyA1IDAgUgovUm9vdCA0IDAgUgovU2l6ZSA4Cj4+CnN0YXJ0eHJlZgo5NTAKJSVFT0YK");

    [SetUp]
    public void SetUp()
    {
        workspace = Path.Combine(Path.GetTempPath(), "brevi-en003-" + Guid.NewGuid().ToString("N"));
        privateRoot = Path.Combine(workspace, "private");
        store = new FileSystemPrivateFileStore(new PrivateFileStorageOptions { RootPath = privateRoot, PublicWebRootPath = Path.Combine(workspace, "wwwroot") });
    }
    [TearDown]
    public void TearDown() { if (Directory.Exists(workspace)) Directory.Delete(workspace, true); }

    [Test]
    public async Task Private_bytes_roundtrip_hash_and_foreign_scope_denial()
    {
        var written = await store.WriteAsync(scope, new MemoryStream(Pdf), "application/pdf", default);
        var read = await store.ReadAsync(scope, written.Value.FileId, default);
        Assert.Multiple(() =>
        {
            Assert.That(read.IsSuccess, Is.True);
            Assert.That(read.Value.Bytes.ToArray(), Is.EqualTo(Pdf));
            Assert.That(read.Value.File.Sha256, Is.EqualTo(Convert.ToHexString(SHA256.HashData(Pdf))));
            Assert.That(read.Value.File.Length, Is.EqualTo(Pdf.Length));
        });
        Assert.That((await store.ReadAsync(Guid.NewGuid(), written.Value.FileId, default)).Status, Is.EqualTo(ResultStatus.NotFound));
    }

    [Test]
    public async Task Foreign_actor_cannot_read_or_write_even_with_known_file_id()
    {
        var written = await store.WriteAsync(scope, new MemoryStream(Pdf), "application/pdf", default);
        var access = new Mock<IPrivateFileAccess>();
        access.Setup(x => x.CanAccessConfigurationAsync(It.IsAny<CrmActor>(), scope, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var service = new PrivateFileService(access.Object, store);
        Assert.That((await service.ReadAsync(new CrmActor(77, false), scope, written.Value.FileId, default)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That((await service.WriteAsync(new CrmActor(77, false), scope, new MemoryStream(Pdf), "application/pdf", default)).Status, Is.EqualTo(ResultStatus.NotFound));
        Assert.That(Directory.GetDirectories(Path.Combine(privateRoot, scope.ToString("N"))).Length, Is.EqualTo(1));
    }

    [Test]
    public async Task Authorized_actor_gets_the_actual_scoped_bytes()
    {
        var access = new Mock<IPrivateFileAccess>();
        access.Setup(x => x.CanAccessConfigurationAsync(It.Is<CrmActor>(a => a.UserId == 1), scope, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var service = new PrivateFileService(access.Object, store);
        var written = await service.WriteAsync(new CrmActor(1, false), scope, new MemoryStream(Pdf), "application/pdf", default);
        Assert.That((await service.ReadAsync(new CrmActor(1, false), scope, written.Value.FileId, default)).Value.Bytes.ToArray(), Is.EqualTo(Pdf));
    }

    [Test]
    public async Task Twentieth_mib_is_accepted_but_one_more_byte_is_rejected()
    {
        var bytes = new byte[PrivateFilePolicy.MaximumBytes + 1];
        Pdf.CopyTo(bytes, 0);
        Assert.That((await store.WriteAsync(scope, new MemoryStream(bytes, 0, PrivateFilePolicy.MaximumBytes), "application/pdf", default)).IsSuccess, Is.True);
        Assert.That((await store.WriteAsync(scope, new MemoryStream(bytes), "application/pdf", default)).Status, Is.EqualTo(ResultStatus.Invalid));
    }

    [Test]
    public async Task Thirty_file_limit_is_atomic_under_competing_writers()
    {
        for (var index = 0; index < 29; index++) Assert.That((await store.WriteAsync(scope, new MemoryStream(Pdf), "application/pdf", default)).IsSuccess, Is.True);
        var writes = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => store.WriteAsync(scope, new MemoryStream(Pdf), "application/pdf", default)));
        Assert.That(writes.Count(x => x.IsSuccess), Is.EqualTo(1));
        Assert.That(writes.Count(x => x.Status == ResultStatus.Invalid), Is.EqualTo(3));
    }

    [Test]
    public async Task Mime_spoof_and_empty_content_do_not_create_a_file()
    {
        Assert.That((await store.WriteAsync(scope, new MemoryStream(Pdf), "image/png", default)).Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That((await store.WriteAsync(scope, new MemoryStream(), "application/pdf", default)).Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(Directory.GetDirectories(privateRoot), Is.Empty);
    }

    [Test]
    public async Task Changed_bytes_are_not_returned_as_verified_content()
    {
        var written = await store.WriteAsync(scope, new MemoryStream(Pdf), "application/pdf", default);
        await File.WriteAllBytesAsync(Path.Combine(privateRoot, scope.ToString("N"), written.Value.FileId.ToString("N"), "content"), "%PDF-tampered"u8.ToArray());
        Assert.That((await store.ReadAsync(scope, written.Value.FileId, default)).Status, Is.EqualTo(ResultStatus.Error));
    }

    [Test]
    public void Unconfigured_root_and_public_tree_are_rejected()
    {
        Assert.Throws<InvalidOperationException>(() => new FileSystemPrivateFileStore(new PrivateFileStorageOptions()));
        Assert.Throws<InvalidOperationException>(() => new FileSystemPrivateFileStore(new PrivateFileStorageOptions { RootPath = Path.Combine(workspace, "wwwroot", "uploads"), PublicWebRootPath = Path.Combine(workspace, "wwwroot") }));
    }

    [Test]
    public void Broad_existing_acl_is_rejected_without_downgrading_privacy()
    {
        if (OperatingSystem.IsWindows())
        {
            var directory = new DirectoryInfo(privateRoot);
            var security = directory.GetAccessControl();
            security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.Read, AccessControlType.Allow));
            directory.SetAccessControl(security);
        }
        else File.SetUnixFileMode(privateRoot, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.OtherRead);
        Assert.Throws<InvalidOperationException>(() => new FileSystemPrivateFileStore(new PrivateFileStorageOptions { RootPath = privateRoot, PublicWebRootPath = Path.Combine(workspace, "wwwroot") }));
    }

    [Test]
    public async Task Cancellation_is_propagated_without_committing_a_file()
    {
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        Assert.ThrowsAsync<TaskCanceledException>(async () => await store.WriteAsync(scope, new MemoryStream(Pdf), "application/pdf", cancellation.Token));
        Assert.That(Directory.GetDirectories(privateRoot), Is.Empty);
        await Task.CompletedTask;
    }
}
