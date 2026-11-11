namespace Reference.Api.Contracts.FabricColors;

/// <summary>A colour of «Кольори»: a new one or the edited fields.</summary>
public sealed record FabricColorRequest(string Name, bool IsActive = true, int SortOrder = 0);
