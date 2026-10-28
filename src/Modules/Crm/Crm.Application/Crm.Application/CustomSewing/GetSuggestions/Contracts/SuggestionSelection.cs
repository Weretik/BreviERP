namespace Crm.Application.CustomSewing.GetSuggestions.Contracts;

public sealed record SuggestionSelection(string Kind, int ReferenceId, string Unit, decimal Quantity, decimal? ManualMinutes);
