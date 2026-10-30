using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Introductions;
using Crm.Domain.Quotes.Items;

namespace Crm.Domain.Quotes.Versions;

public sealed class QuoteVersion
{
    public long Id { get; private set; }
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public QuoteId QuoteId { get; private set; }
    public int VersionNumber { get; private set; }
    public long? ParentVersionId { get; private set; }
    public string Action { get; private set; } = null!;
    public string RecipientName { get; private set; } = null!;
    public Guid? CustomerPublicId { get; private set; }
    public Guid? ContactPublicId { get; private set; }
    public string Currency { get; private set; } = "UAH";
    public string TaxBasis { get; private set; } = "no_extra_vat";
    public int ActorId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public IntroductionPublicSnapshot? IntroductionPublicSnapshot { get; private set; }
    private readonly List<QuoteItem> _items = [];
    public IReadOnlyCollection<QuoteItem> Items => _items;

    private QuoteVersion() { }

    public static QuoteVersion Create(
        QuoteId quoteId, int number, long? parentVersionId, string action,
        string recipientName, Guid? customerPublicId, Guid? contactPublicId,
        int actorId, IEnumerable<QuoteItem> items,
        IntroductionPublicSnapshot? introductionPublicSnapshot = null)
    {
        if (quoteId.Value <= 0 || number <= 0 || actorId <= 0 ||
            action is not ("created" or "edited" or "recalculated" or "copied"))
            throw new QuoteValidationException("invalid_version");
        var version = new QuoteVersion
        {
            QuoteId = quoteId,
            VersionNumber = number,
            ParentVersionId = parentVersionId,
            Action = action,
            RecipientName = recipientName,
            CustomerPublicId = customerPublicId,
            ContactPublicId = contactPublicId,
            ActorId = actorId,
            IntroductionPublicSnapshot = introductionPublicSnapshot is null ? null :
                introductionPublicSnapshot with
                {
                    Slots = Array.AsReadOnly(introductionPublicSnapshot.Slots.ToArray())
                }
        };
        version._items.AddRange(items);
        if (version._items.Count > 100 || version._items.Select(x => x.SortOrder).Distinct().Count() != version._items.Count)
            throw new QuoteValidationException("invalid_item_order");
        return version;
    }
}
