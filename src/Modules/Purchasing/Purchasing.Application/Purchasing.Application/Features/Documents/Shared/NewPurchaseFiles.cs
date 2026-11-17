using Purchasing.Domain.Files;
using Purchasing.Application.Abstractions;
using Purchasing.Domain.Ingestion.Normalization;

namespace Purchasing.Application.Features.Documents.Shared;

/// <summary>
/// The files of «Розпізнати всі нові»: active safe originals never checked or tried before whose message still has an
/// active row, newest messages first.
/// </summary>
public static class NewPurchaseFiles
{
    /// <summary>Safe links are checked in memory: the rule is code, not SQL.</summary>
    public static async Task<List<PurchaseMessageFileId>> IdsAsync(IPurchasingDbContext db, CancellationToken cancellationToken)
    {
        var files = await (
                from file in db.Files
                join message in db.Messages on file.MessageId equals message.Id
                where file.Active
                      && !db.CheckedDocuments.Any(x => x.Id == file.Id)
                      && !db.RecognitionAttempts.Any(x => x.FileId == file.Id)
                      && db.SourceRows.Any(x => x.Active && x.MessageId == file.MessageId)
                orderby message.MessageDate == null, message.MessageDate descending, file.Id
                select new { file.Id, file.Url })
            .ToListAsync(cancellationToken);
        return files.Where(x => FileLinkRules.IsSafeDropbox(x.Url)).Select(x => x.Id).ToList();
    }
}
