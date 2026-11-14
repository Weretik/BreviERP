using System.Text.Json.Serialization;
using Purchasing.Application.Features.Documents.Shared;

namespace Purchasing.Application.Features.Register.Shared;

/// <summary>Checked facts of one original; values are decimal strings exactly as saved.</summary>
public sealed record PurchaseDocumentResponse(
    string DocumentType,
    string DocumentNumber,
    string DocumentDate,
    string SupplierName,
    PurchaseDocumentVat Vat,
    bool PricesIncludeVat,
    string? VatAmount,
    string TotalPayable,
    IReadOnlyList<PurchaseDocumentItemResponse> Items,
    PurchaseDocumentSource Source,
    string? Model,
    string CheckedBy,
    DateTimeOffset CheckedAt);
