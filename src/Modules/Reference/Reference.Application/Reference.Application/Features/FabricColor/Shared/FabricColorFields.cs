namespace Reference.Application.Features.FabricColor.Shared;

/// <summary>Editable fields of a colour of «Кольори».</summary>
public sealed record FabricColorFields(string Name, bool IsActive = true, int SortOrder = 0);
