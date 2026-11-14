using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;

namespace Accounting.Application.Features.Categories.Commands.DeleteFinanceCategory;

/// <summary>
/// «Видалити» a category; a root goes with its subcategories, each with its own log entry. When any operation (cancelled
/// ones too) uses the category or one of its subcategories, the whole branch is only switched off
/// (<see cref="AuditAction.Deactivate"/>) and keeps its ids, so old operations still show it.
/// </summary>
public sealed record DeleteFinanceCategoryCommand(Guid Id, int Version, string By) : ICommand<Result>;
