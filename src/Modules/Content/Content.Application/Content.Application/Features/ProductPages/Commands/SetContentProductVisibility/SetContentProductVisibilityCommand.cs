using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Application.Features.ProductPages.Shared;

namespace Content.Application.Features.ProductPages.Commands.SetContentProductVisibility;

/// <summary>
/// «Показувати на сайті» from the list (CNT-002 SC-03). A product without a page gets an empty one with an address
/// built from its name.
/// </summary>
public sealed record SetContentProductVisibilityCommand(int ProductId, bool ShowOnSite, string UpdatedBy)
    : ICommand<Result<ContentProductVisibilityResponse>>;
