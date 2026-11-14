using System.Text.Json.Serialization;

namespace Purchasing.Application.Features.Documents.Shared;

[JsonConverter(typeof(JsonStringEnumConverter<PurchaseDocumentSource>))]
public enum PurchaseDocumentSource
{
    Ai,
    Manual,
}
