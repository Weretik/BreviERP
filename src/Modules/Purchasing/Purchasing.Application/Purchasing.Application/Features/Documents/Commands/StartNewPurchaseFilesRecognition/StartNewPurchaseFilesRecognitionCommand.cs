namespace Purchasing.Application.Features.Documents.Commands.StartNewPurchaseFilesRecognition;

/// <summary>
/// «Розпізнати всі нові»: when there are new originals, asks the scheduler to start the «Розпізнавання оригіналів
/// закупки» background task; the next tick, within a minute, starts it. Conflict when it already runs or waits.
/// </summary>
public sealed record StartNewPurchaseFilesRecognitionCommand(string By)
    : ICommand<Result<PurchaseRecognitionStartResponse>>;
