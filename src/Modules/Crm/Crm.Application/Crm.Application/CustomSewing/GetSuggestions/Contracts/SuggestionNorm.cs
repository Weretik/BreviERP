namespace Crm.Application.CustomSewing.GetSuggestions.Contracts;

public sealed record SuggestionNorm(string Kind, int ReferenceId, string Unit, decimal? ManualMinutes,
    int SampleSize, decimal Quantity, decimal MinimumQuantity, decimal MaximumQuantity, bool VaryingNorm);
