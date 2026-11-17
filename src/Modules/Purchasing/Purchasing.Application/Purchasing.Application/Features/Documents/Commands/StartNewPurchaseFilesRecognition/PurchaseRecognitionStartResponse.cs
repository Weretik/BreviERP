namespace Purchasing.Application.Features.Documents.Commands.StartNewPurchaseFilesRecognition;

/// <summary>
/// <paramref name="Waiting"/> new originals wait for recognition; <paramref name="Started"/> is false when there were
/// none, so nothing was started.
/// </summary>
public sealed record PurchaseRecognitionStartResponse(int Waiting, bool Started);
