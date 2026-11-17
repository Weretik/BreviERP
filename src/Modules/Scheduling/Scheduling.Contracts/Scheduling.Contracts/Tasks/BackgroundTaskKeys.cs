namespace Scheduling.Contracts.Tasks;

/// <summary>Keys of the «Фонові задачі» tasks that other modules start through <see cref="IBackgroundTaskRequests"/>.</summary>
public static class BackgroundTaskKeys
{
    /// <summary>«Розпізнати всі нові» of «Рахунки на закупку»: AI recognition of the new purchase originals.</summary>
    public const string PurchaseRecognition = "purchase-recognition";
}
