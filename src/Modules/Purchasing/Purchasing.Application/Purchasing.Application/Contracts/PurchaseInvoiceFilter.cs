using System.Text.Json.Serialization;

namespace Purchasing.Application.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter<PurchaseInvoiceFilter>))]
public enum PurchaseInvoiceFilter
{
    All,
    NeedsReview,
}
