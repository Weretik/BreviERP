using Purchasing.Domain.Documents;
using Purchasing.Application.Features.Register.Shared;
using Purchasing.Application.Features.Documents.Shared.Recognition;

namespace Purchasing.Application.Features.Documents.Shared;

internal static class PurchaseDocumentMapping
{
    public static PurchaseRecognitionResult ToContract(PurchaseRecognitionOutcome outcome) => outcome switch
    {
        PurchaseRecognitionOutcome.Recognized => PurchaseRecognitionResult.Recognized,
        PurchaseRecognitionOutcome.NeedsReview => PurchaseRecognitionResult.NeedsReview,
        PurchaseRecognitionOutcome.Skipped => PurchaseRecognitionResult.Skipped,
        _ => PurchaseRecognitionResult.Failed,
    };

    /// <summary>Null when the stored payload no longer passes the rules (it is then reported as a problem).</summary>
    public static PurchaseDocumentResponse? ToResponse(PurchaseCheckedDocument stored)
    {
        if (PurchaseDocumentJson.Read(stored.PayloadJson) is not { } document) return null;

        return new PurchaseDocumentResponse(document.DocumentType, document.DocumentNumber, document.DocumentDate,
            document.SupplierName, document.Vat == PurchaseVatMode.WithVat ? PurchaseDocumentVat.WithVat : PurchaseDocumentVat.WithoutVat,
            document.PricesIncludeVat, document.VatAmount, document.TotalPayable,
            document.Items.Select(x => new PurchaseDocumentItemResponse(x.Position, x.Name, x.Quantity, x.Unit, x.Price, x.Sum)).ToArray(),
            stored.Origin == PurchaseDocumentOrigin.Ai ? PurchaseDocumentSource.Ai : PurchaseDocumentSource.Manual,
            stored.Model, stored.CheckedBy, stored.CheckedAt);
    }

    public static PurchaseRecognitionAttemptResponse ToResponse(PurchaseRecognitionAttempt attempt) => new(
        ToContract(attempt.Outcome), attempt.Problems, attempt.Model, attempt.CostUsd, attempt.By, attempt.At);
}
