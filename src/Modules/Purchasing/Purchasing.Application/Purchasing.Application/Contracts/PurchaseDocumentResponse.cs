using System.Text.Json.Serialization;

namespace Purchasing.Application.Contracts;

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

public sealed record PurchaseDocumentItemResponse(int Position, string Name, string Quantity, string? Unit, string Price, string Sum);

/// <summary>«З ПДВ» or «Без ПДВ» (when VAT is not clearly printed).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<PurchaseDocumentVat>))]
public enum PurchaseDocumentVat
{
    WithVat,
    WithoutVat,
}

[JsonConverter(typeof(JsonStringEnumConverter<PurchaseDocumentSource>))]
public enum PurchaseDocumentSource
{
    Ai,
    Manual,
}

/// <summary>The last «Розпізнати» of a file that has no checked document yet.</summary>
public sealed record PurchaseRecognitionAttemptResponse(
    PurchaseRecognitionResult Result,
    IReadOnlyList<string> Problems,
    string? Model,
    decimal CostUsd,
    string By,
    DateTimeOffset At);

[JsonConverter(typeof(JsonStringEnumConverter<PurchaseRecognitionResult>))]
public enum PurchaseRecognitionResult
{
    Recognized,
    NeedsReview,
    Failed,
    Skipped,
}
