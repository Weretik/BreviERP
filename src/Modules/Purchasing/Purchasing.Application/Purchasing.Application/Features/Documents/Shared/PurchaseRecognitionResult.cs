using System.Text.Json.Serialization;

namespace Purchasing.Application.Features.Documents.Shared;

[JsonConverter(typeof(JsonStringEnumConverter<PurchaseRecognitionResult>))]
public enum PurchaseRecognitionResult
{
    Recognized,
    NeedsReview,
    Failed,
    Skipped,
}
