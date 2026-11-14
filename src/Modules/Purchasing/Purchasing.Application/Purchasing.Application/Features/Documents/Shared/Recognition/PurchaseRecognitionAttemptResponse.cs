using System.Text.Json.Serialization;

namespace Purchasing.Application.Features.Documents.Shared.Recognition;

/// <summary>The last «Розпізнати» of a file that has no checked document yet.</summary>
public sealed record PurchaseRecognitionAttemptResponse(
    PurchaseRecognitionResult Result,
    IReadOnlyList<string> Problems,
    string? Model,
    decimal CostUsd,
    string By,
    DateTimeOffset At);
