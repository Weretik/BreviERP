using System.Text.Json.Serialization;

namespace Content.Domain.ProductPages;

/// <summary>A short fact next to the photo: «Сезон» — «весна, літо, осінь». The label may be empty for a plain line.</summary>
public sealed record ProductFact(string? Label, string Text);

/// <summary>Written by name in JSON, both in the API and in the stored texts.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ProductTextBlockKind>))]
public enum ProductTextBlockKind
{
    Paragraph = 1,
    Heading = 2,
    List = 3,
}

/// <summary>
/// One block of the product description. A paragraph may open with a bold <see cref="Lead"/>; a list keeps its lines in
/// <see cref="Items"/> and has no <see cref="Text"/>.
/// </summary>
public sealed record ProductTextBlock(ProductTextBlockKind Kind, string? Text, string? Lead, IReadOnlyList<string>? Items);

public sealed record ProductSpecRow(string Name, string Value);

/// <summary>Characteristics of one garment part: «Куртка», «Брюки».</summary>
public sealed record ProductSpecGroup(string? Title, IReadOnlyList<ProductSpecRow> Rows);

/// <summary>What staff type for one language of a product page.</summary>
public sealed record ProductPageTextInput(
    string? Name,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ProductFact>? Summary,
    IReadOnlyList<ProductTextBlock>? Description,
    IReadOnlyList<ProductSpecGroup>? Specs);
