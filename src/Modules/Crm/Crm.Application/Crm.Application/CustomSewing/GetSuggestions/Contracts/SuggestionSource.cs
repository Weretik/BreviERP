namespace Crm.Application.CustomSewing.GetSuggestions.Contracts;

public sealed record SuggestionSource(IReadOnlyList<SuggestionInput> Models, bool LimitReached);
