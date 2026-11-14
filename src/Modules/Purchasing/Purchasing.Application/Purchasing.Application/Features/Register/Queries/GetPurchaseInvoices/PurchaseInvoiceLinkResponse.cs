using Purchasing.Application.Features.Documents.Shared;
using Purchasing.Application.Features.Register.Shared;
namespace Purchasing.Application.Features.Register.Queries.GetPurchaseInvoices;

/// <summary>
/// An original-document link; <see cref="Safe"/> is false for anything but an HTTPS Dropbox link. <see cref="Document"/> is
/// its checked facts; <see cref="Recognition"/> is the last failed recognition while there are none.
/// </summary>
public sealed record PurchaseInvoiceLinkResponse(
    string Id,
    string Url,
    bool Safe,
    PurchaseDocumentResponse? Document,
    PurchaseRecognitionAttemptResponse? Recognition);
