using System.Text.Json.Serialization;

namespace Purchasing.Application.Features.Register.Shared;

/// <summary>«З ПДВ» or «Без ПДВ» (when VAT is not clearly printed).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<PurchaseDocumentVat>))]
public enum PurchaseDocumentVat
{
    WithVat,
    WithoutVat,
}
