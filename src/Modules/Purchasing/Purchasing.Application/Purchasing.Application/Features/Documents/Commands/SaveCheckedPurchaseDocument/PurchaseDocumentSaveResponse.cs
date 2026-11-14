namespace Purchasing.Application.Features.Documents.Commands.SaveCheckedPurchaseDocument;

/// <summary>Saving a checked document by hand: <see cref="Unchanged"/> when the same facts were already saved.</summary>
public sealed record PurchaseDocumentSaveResponse(string FileId, bool Unchanged, bool Replaced);
