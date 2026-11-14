namespace Reference.Contracts.Fabrics;

/// <summary>A row of «Тканини» as other modules see it.</summary>
public sealed record FabricInfo(
    int Id, string? Code, string Name, decimal Price, string? Composition, string? Density, int ProviderId, string ProviderName,
    string? ProposalDescription, bool IsActive)
{
    /// <summary>What a client reads under the fabric: «Короткий опис для КП», else composition and density.</summary>
    public string? ClientDetails
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(ProposalDescription)) return ProposalDescription.Trim();
            var parts = new[] { Composition, Density is { Length: > 0 } density ? $"щільність {density}" : null }
                .Where(x => !string.IsNullOrWhiteSpace(x));
            var text = string.Join(", ", parts);
            return text.Length == 0 ? null : text;
        }
    }
}
