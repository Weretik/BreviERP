namespace Purchasing.Application.Features.Documents.Shared.Recognition;

/// <summary>Result of «Розпізнати» for one file.</summary>
public sealed record PurchaseRecognitionResponse(
    string FileId,
    PurchaseRecognitionResult Result,
    IReadOnlyList<string> Problems,
    string? Model,
    decimal CostUsd);
