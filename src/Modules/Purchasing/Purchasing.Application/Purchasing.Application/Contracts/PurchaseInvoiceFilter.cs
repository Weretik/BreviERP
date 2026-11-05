using System.Text.Json.Serialization;

namespace Purchasing.Application.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter<PurchaseInvoiceFilter>))]
public enum PurchaseInvoiceFilter
{
    All,

    /// <summary>The representative row has problems, or the last recognition of a file failed.</summary>
    NeedsReview,

    /// <summary>No supplier linked by hand yet.</summary>
    WithoutSupplier,

    /// <summary>A safe link has no checked facts yet.</summary>
    NotRecognized,
}
