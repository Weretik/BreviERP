using Content.Application.Abstractions;
using Content.Domain.SiteCategories;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Commands.ReorderSiteCategories;

/// <summary>
/// The new order of the categories of one parent (null for the top level). <paramref name="Ids"/> must list exactly
/// the categories of that parent; otherwise the list was changed meanwhile and nothing is saved.
/// </summary>
public sealed record ReorderSiteCategoriesCommand(int? ParentId, IReadOnlyList<int> Ids) : ICommand<Result>;
