using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Categories.Shared;

namespace Accounting.Application.Features.Categories.Commands.UpdateFinanceCategory;

/// <summary>«Змінити категорію»: only the name, note and order; the direction, area and root stay.</summary>
public sealed record UpdateFinanceCategoryCommand(Guid Id, string? Name, string? Note, int SortOrder, int Version, string By)
    : ICommand<Result<FinanceCategoryResponse>>;
