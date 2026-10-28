using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Calculate;
using Crm.Application.CustomSewing.Contracts;
using Crm.Application.CustomSewing.CreateDraft;
using Crm.Application.CustomSewing.SaveVersion;
using Crm.Application.Files.Contracts;
using Crm.Application.Files.Finalize;
using Crm.Application.Files.Upload;
using Crm.Infrastructure.Files.Finalize;
using Crm.Infrastructure.Files.Storage;
using Crm.Infrastructure.Files.Upload;
using Crm.Infrastructure.Files.Verification;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.CustomSewing.Writing;
using IntegrationTests.Crm.CustomSewing;

namespace IntegrationTests.Crm.Files.Support;

public abstract class FileLifecycleFixture : CrmDatabaseFixture
{
    protected static readonly CrmActor Manager = new(17, false);
    protected string Workspace = null!;
    protected ConfiguredPrivateFileStore Bytes = null!;
    internal FileLifecycleClock Clock = null!;
    protected PrivateUploadOptions Options = null!;

    [SetUp]
    public void CreateFileWorkspace()
    {
        Workspace = Path.Combine(Path.GetTempPath(), "brevi-ts007-" + Guid.NewGuid().ToString("N"));
        Clock = new FileLifecycleClock();
        Options = new PrivateUploadOptions();
        Bytes = new ConfiguredPrivateFileStore(new PrivateFileStorageOptions
        {
            RootPath = Path.Combine(Workspace, "private"), PublicWebRootPath = Path.Combine(Workspace, "wwwroot")
        }, Clock);
    }

    [TearDown]
    public void DeleteFileWorkspace()
    {
        if (Directory.Exists(Workspace)) Directory.Delete(Workspace, true);
    }

    protected static async Task<CustomDetail> Create(CrmDbContext db)
        => (await new CreateCustomDraftCommandHandler(new CustomMutationStore(db)).Handle(
            new(Guid.NewGuid(), null, null, CustomSewingPersistenceTests.Configuration, Manager), default)).Value;

    protected Task<Result<PrivateFileView>> Upload(CrmDbContext db, Guid draftId, Guid? requestId = null,
        CrmActor? actor = null, byte[]? bytes = null, string type = "application/pdf")
        => new CustomFileUploader(db, Bytes, Bytes, Options, Clock).UploadAsync(
            new UploadPrivateFileCommand(draftId, requestId ?? Guid.NewGuid(), new MemoryStream(bytes ?? FileLifecycleTestData.Pdf), type,
                actor ?? Manager), default);

    protected Task<Result<CustomDetail>> SetFiles(CrmDbContext db, CustomDetail draft, IReadOnlyList<CustomFileSelection> files,
        Guid? requestId = null, CrmActor? actor = null)
        => new SetCustomFilesCommandHandler(new CustomMutationStore(db), new FinalizedCustomFilesWriter(db, Bytes, Clock))
            .Handle(new(draft.PublicId, requestId ?? Guid.NewGuid(), draft.Revision, files, actor ?? Manager), default).AsTask();

    protected async Task<CustomDetail> Freeze(CrmDbContext db, CustomDetail draft)
    {
        var prices = new CustomSewingTestPrices();
        var verifier = new CustomArtworkVerifier(db, Bytes, Clock);
        var preview = await new CalculateCustomSewingCommandHandler(new CustomMutationStore(db), prices, verifier)
            .Handle(new(draft.PublicId, draft.Revision, Guid.NewGuid(), Manager), default);
        Assert.That(preview.IsSuccess, Is.True, string.Join(",", preview.Errors));
        var saved = await new SaveCustomVersionCommandHandler(new CustomMutationStore(db), prices, null, verifier)
            .Handle(new(draft.PublicId, draft.Revision, Guid.NewGuid(), preview.Value.Preview!.Token, Manager), default);
        Assert.That(saved.IsSuccess, Is.True, string.Join(",", saved.Errors));
        return saved.Value;
    }
}
