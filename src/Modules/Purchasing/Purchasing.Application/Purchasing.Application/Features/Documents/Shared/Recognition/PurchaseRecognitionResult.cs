using System.Text.Json.Serialization;

namespace Purchasing.Application.Features.Documents.Shared.Recognition;

[JsonConverter(typeof(JsonStringEnumConverter<PurchaseRecognitionResult>))]
public enum PurchaseRecognitionResult
{
    Recognized,
    NeedsReview,
    Failed,
    Skipped,
}
