using System.Text.Json.Serialization;

namespace Purchasing.Application.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter<PurchaseInvoiceFilter>))]
public enum PurchaseInvoiceFilter
{
    All,

    /// <summary>The representative row has problems.</summary>
    NeedsReview,

    /// <summary>No supplier linked by hand yet.</summary>
    WithoutSupplier,
}
