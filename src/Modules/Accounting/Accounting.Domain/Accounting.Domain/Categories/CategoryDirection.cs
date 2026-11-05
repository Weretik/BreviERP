using System.Text.Json.Serialization;

namespace Accounting.Domain.Categories;

/// <summary>«Категорії доходів» or «Категорії витрат».</summary>
[JsonConverter(typeof(JsonStringEnumConverter<CategoryDirection>))]
public enum CategoryDirection
{
    Income,
    Expense,
}
