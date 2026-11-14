using Reference.Application.Features.FabricColor.Shared;
using Reference.Application.Features.FabricColor.Commands.CreateFabricColor;

namespace Reference.Api.Endpoints.FabricColors.Shared;

/// <summary>A colour of «Кольори»: a new one or the edited fields.</summary>
public sealed record FabricColorRequest(string Name, bool IsActive = true, int SortOrder = 0);
