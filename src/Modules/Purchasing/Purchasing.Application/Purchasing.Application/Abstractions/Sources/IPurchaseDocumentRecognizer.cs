namespace Purchasing.Application.Abstractions.Sources;

/// <summary>Reads a document with the AI of «Налаштування ШІ»; implemented in the host through the Ai module.</summary>
public interface IPurchaseDocumentRecognizer
{
    Task<PurchaseRecognitionAnswer> RecognizeAsync(PurchaseRecognitionRequest request, CancellationToken cancellationToken);
}

public sealed record PurchaseRecognitionRequest(PurchaseFileContent File, bool Strong, string Instructions, string Text,
    string SchemaName, string SchemaJson);

public enum PurchaseRecognitionStatus
{
    Answered,

    /// <summary>The AI is off, has no key or reached its monthly limit; nothing was sent.</summary>
    Unavailable,

    /// <summary>OpenAI refused or did not answer.</summary>
    Failed,
}

public sealed record PurchaseRecognitionAnswer(PurchaseRecognitionStatus Status, string? Json, string? Model, decimal CostUsd,
    string? Message);
