namespace Crm.Application.CustomSewing.GetSuggestions.Contracts;

public sealed record SuggestionInput(string ModelCode, string ProductType, string Season, bool Approved, IReadOnlyList<SuggestionSelection> Selections);
