using System.Text.Json;
using Purchasing.Application.Abstractions;
using Purchasing.Domain.Documents;
using Purchasing.Application.Features.Documents.Shared;
using Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;

namespace Purchasing.Application.Features.Documents.Commands.SaveCheckedPurchaseDocument;

public sealed class SaveCheckedPurchaseDocumentHandler(IPurchasingDbContext db, TimeProvider clock)
    : ICommandHandler<SaveCheckedPurchaseDocumentCommand, Result<PurchaseDocumentSaveResponse>>
{
    public const string NeedsReplace = "Для цього файлу вже збережено інші дані. Підтвердіть заміну.";
    public const string WrongUid = "sourceUID не належить до активних рядків цього повідомлення.";

    public async ValueTask<Result<PurchaseDocumentSaveResponse>> Handle(SaveCheckedPurchaseDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var file = await db.Files.AsNoTracking().FirstOrDefaultAsync(x => x.Id == command.FileId && x.Active, cancellationToken);
        if (file is null) return Result<PurchaseDocumentSaveResponse>.NotFound(PurchaseFileRecognition.FileNotFound);

        var uid = command.Document.ValueKind == JsonValueKind.Object
                  && command.Document.TryGetProperty("sourceUID", out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()!
            : string.Empty;
        if (!await db.SourceRows.AnyAsync(x => x.Active && x.MessageId == file.MessageId && x.SourceUid == uid, cancellationToken))
            return Result<PurchaseDocumentSaveResponse>.Invalid(new ValidationError("sourceUID", WrongUid));

        var (document, problems) = PurchaseDocumentRules.Read(command.Document, uid, file.Url, identityInJson: true);
        if (document is null)
            return Result<PurchaseDocumentSaveResponse>.Invalid(problems.Select(x => new ValidationError("document", x)).ToArray());

        var now = clock.GetUtcNow();
        await using var scope = await db.TryBeginWriteAsync(cancellationToken);
        if (scope is null) return Result<PurchaseDocumentSaveResponse>.Conflict(PurchaseRegisterSync.AlreadyRunning);

        var current = await db.CheckedDocuments.FirstOrDefaultAsync(x => x.Id == file.Id, cancellationToken);
        var replaced = false;
        if (current is null)
            db.CheckedDocuments.Add(new PurchaseCheckedDocument(file.Id, file.MessageId, document, PurchaseDocumentOrigin.Manual,
                null, null, command.By, now));
        else if (current.PayloadHash == PurchaseDocumentJson.Hash(PurchaseDocumentJson.Write(document)))
            return Result.Success(new PurchaseDocumentSaveResponse(file.Id, Unchanged: true, Replaced: false));
        else if (!command.Replace)
            return Result<PurchaseDocumentSaveResponse>.Conflict(NeedsReplace);
        else
        {
            db.CheckedDocumentHistory.Add(current.Replace(document, PurchaseDocumentOrigin.Manual, null, null, command.By, now));
            replaced = true;
        }

        await db.SaveChangesAsync(cancellationToken);
        await scope.CommitAsync(cancellationToken);
        return Result.Success(new PurchaseDocumentSaveResponse(file.Id, Unchanged: false, Replaced: replaced));
    }
}
