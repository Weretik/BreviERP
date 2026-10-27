using Crm.Domain.Quotes.Variants;

namespace Crm.Domain.Quotes.Items;

public sealed class QuoteItem
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public long VersionId { get; private set; }
    public int SortOrder { get; private set; }
    public string Title { get; private set; } = null!;
    public string? PublicDescription { get; private set; }
    public string? PublicComment { get; private set; }
    public string? InternalNote { get; private set; }
    public bool Included { get; private set; }
    private readonly List<QuoteVariant> _variants = [];
    public IReadOnlyCollection<QuoteVariant> Variants => _variants;

    private QuoteItem() { }

    public static QuoteItem Create(
        Guid publicId, int sortOrder, string title, string? description,
        string? comment, string? internalNote, bool included, IEnumerable<QuoteVariant> variants)
    {
        if (sortOrder < 0 || string.IsNullOrWhiteSpace(title) || title.Length > 300 ||
            description?.Length > 5000 || comment?.Length > 5000 || internalNote?.Length > 5000)
            throw new QuoteValidationException("invalid_item");
        var item = new QuoteItem
        {
            PublicId = publicId == Guid.Empty ? Guid.NewGuid() : publicId,
            SortOrder = sortOrder,
            Title = title.Trim(),
            PublicDescription = description,
            PublicComment = comment,
            InternalNote = internalNote,
            Included = included
        };
        item._variants.AddRange(variants);
        if (item._variants.Count > 30 ||
            item._variants.Select(x => x.SortOrder).Distinct().Count() != item._variants.Count)
            throw new QuoteValidationException("invalid_variant_order");
        return item;
    }
}
