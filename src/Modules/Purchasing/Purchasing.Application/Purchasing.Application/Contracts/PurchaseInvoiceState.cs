using System.Text.Json.Serialization;

namespace Purchasing.Application.Contracts;

/// <summary>Computed card state: problems of the representative row win over everything else.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<PurchaseInvoiceState>))]
public enum PurchaseInvoiceState
{
    NeedsReview,
    AwaitingRecognition,
}
