namespace Purchasing.Domain.Documents;

/// <summary>
/// One press of «Розпізнати» for a file (fast model, then the strong one if needed). Its problems show on the card until a
/// checked document exists. Append-only.
/// </summary>
public sealed class PurchaseRecognitionAttempt
{
    public const int ProblemMaxLength = 500;
    public const int MaxProblems = 20;
    public const int OutputMaxLength = 200_000;

    private PurchaseRecognitionAttempt() { }

    public PurchaseRecognitionAttempt(string fileId, string messageId, PurchaseRecognitionOutcome outcome,
        IEnumerable<string> problems, string? model, decimal costUsd, string? outputJson, string by, DateTimeOffset at)
    {
        FileId = fileId;
        MessageId = messageId;
        Outcome = outcome;
        Problems = problems.Take(MaxProblems).Select(x => x.Length > ProblemMaxLength ? x[..ProblemMaxLength] : x).ToArray();
        Model = model;
        CostUsd = costUsd;
        OutputJson = outputJson is { Length: > OutputMaxLength } ? null : outputJson;
        By = by.Length > PurchaseCheckedDocument.ByMaxLength ? by[..PurchaseCheckedDocument.ByMaxLength] : by;
        At = at;
    }

    public long Id { get; private set; }
    public string FileId { get; private set; } = null!;
    public string MessageId { get; private set; } = null!;
    public PurchaseRecognitionOutcome Outcome { get; private set; }
    public string[] Problems { get; private set; } = [];

    /// <summary>The last model used; null when nothing was sent to OpenAI.</summary>
    public string? Model { get; private set; }

    /// <summary>Cost of all tries of this attempt.</summary>
    public decimal CostUsd { get; private set; }

    /// <summary>The last AI answer when it failed the checks: the start of a manual fix.</summary>
    public string? OutputJson { get; private set; }

    public string By { get; private set; } = null!;
    public DateTimeOffset At { get; private set; }
}

public enum PurchaseRecognitionOutcome
{
    /// <summary>Saved as a checked document.</summary>
    Recognized,

    /// <summary>Both models answered, but the answer failed the checks.</summary>
    NeedsReview,

    /// <summary>The file could not be downloaded or OpenAI failed.</summary>
    Failed,

    /// <summary>The file type cannot be read by AI (Excel, Word): upload JSON by hand.</summary>
    Skipped,
}
