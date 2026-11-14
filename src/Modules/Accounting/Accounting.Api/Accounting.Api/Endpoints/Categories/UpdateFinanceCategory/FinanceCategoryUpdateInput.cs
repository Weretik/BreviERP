using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;

namespace Accounting.Api.Endpoints.Categories.UpdateFinanceCategory;

/// <summary>Name, note and order with the <c>Version</c> the user opened.</summary>
public sealed record FinanceCategoryUpdateInput(string? Name, string? Note, int SortOrder, int Version);
