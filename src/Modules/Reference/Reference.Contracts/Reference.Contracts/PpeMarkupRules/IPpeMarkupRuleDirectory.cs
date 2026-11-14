namespace Reference.Contracts.PpeMarkupRules;

/// <summary>PPE markup rules for other modules.</summary>
public interface IPpeMarkupRuleDirectory
{
    /// <summary>All rows, switched-off ones included.</summary>
    Task<IReadOnlyList<PpeMarkupRuleInfo>> ListAsync(CancellationToken cancellationToken);
}
