using System.Text.Json.Serialization;

namespace Purchasing.Application.Features.Register.Queries.GetPurchaseInvoices.Results;

/// <summary>
/// Computed card state: problems of the representative row or of a recognition win; Recognized when every safe link has
/// checked facts. Not shown as a badge (problems only).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<PurchaseInvoiceState>))]
public enum PurchaseInvoiceState
{
    NeedsReview,
    AwaitingRecognition,
    Recognized,
}
