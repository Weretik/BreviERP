using System.Text.Json.Serialization;
using Content.Domain.ProductPages;

namespace Content.Domain.Articles;

/// <summary>Written by name in JSON, both in the API and in the stored texts; stored numbers still read.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ArticleBlockKind>))]
public enum ArticleBlockKind
{
    Paragraph = 1,
    Heading = 2,
    List = 3,
    Image = 4,
}

/// <summary>
/// One block of an article body. A paragraph may open with a bold <see cref="Lead"/>; a list keeps its lines in
/// <see cref="Items"/>; an image names a photo of the «Фото» library by <see cref="PhotoId"/> with its <see cref="Alt"/>.
/// </summary>
public sealed record ArticleBlock(
    ArticleBlockKind Kind,
    string? Text,
    string? Lead,
    IReadOnlyList<string>? Items,
    int? PhotoId = null,
    string? Alt = null);

/// <summary>What staff type for one language of an article.</summary>
public sealed record ArticleTextInput(
    string? Title,
    string? Excerpt,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ArticleBlock>? Body);

public static class ArticleBlocks
{
    /// <summary>Trims every block and drops the empty ones.</summary>
    public static IReadOnlyList<ArticleBlock> Clean(IReadOnlyList<ArticleBlock>? blocks) =>
        (blocks ?? []).Select(Block).Where(x => x is not null).Select(x => x!).ToArray();

    /// <summary>Photos the blocks show, each once, in reading order.</summary>
    public static IEnumerable<int> PhotoIds(IEnumerable<ArticleBlock> blocks) =>
        blocks.Where(x => x is { Kind: ArticleBlockKind.Image, PhotoId: not null }).Select(x => x.PhotoId!.Value).Distinct();

    private static ArticleBlock? Block(ArticleBlock block)
    {
        switch (block.Kind)
        {
            case ArticleBlockKind.Image:
                return block.PhotoId is > 0
                    ? new ArticleBlock(ArticleBlockKind.Image, null, null, null, block.PhotoId, ProductPageRules.Clean(block.Alt))
                    : null;
            case ArticleBlockKind.List:
                var items = (block.Items ?? []).Select(ProductPageRules.Clean).Where(x => x is not null).Select(x => x!).ToArray();
                return items.Length == 0 ? null : new ArticleBlock(ArticleBlockKind.List, null, null, items);
            default:
                var text = ProductPageRules.Clean(block.Text);
                if (text is null) return null;
                var lead = block.Kind == ArticleBlockKind.Paragraph ? ProductPageRules.Clean(block.Lead) : null;
                return new ArticleBlock(block.Kind, text, lead, null);
        }
    }
}
