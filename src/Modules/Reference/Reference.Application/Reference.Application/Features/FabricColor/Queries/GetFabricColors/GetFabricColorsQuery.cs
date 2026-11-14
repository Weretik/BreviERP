namespace Reference.Application.Features.FabricColor.Queries.GetFabricColors;

/// <summary>All colours of «Кольори» in list order; an empty list when there are none.</summary>
public sealed record GetFabricColorsQuery : IQuery<Result<List<FabricColorRowDTO>>>;

public sealed record FabricColorRowDTO(int Id, string Name, bool IsActive, int SortOrder);
