namespace Reference.Domain.FabricColors;

/// <summary>Editable fields of a colour: one row of the sheet column «Загальний перелік кольорів».</summary>
public sealed record FabricColorDetails(string Name, bool IsActive, int SortOrder);
