namespace Purchasing.Application.Abstractions;

/// <summary>
/// Starts the «Розпізнавання оригіналів закупки» background task (Scheduling): the tasks job reads the new originals
/// with the AI, so many minutes of model calls never run inside an HTTP request. Conflict when it already runs or waits.
/// </summary>
public interface IPurchaseRecognitionLauncher
{
    Task<Result> RequestAsync(string by, CancellationToken cancellationToken);
}
