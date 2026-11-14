using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Building;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.Enums;
using Crm.Domain.Quotes.Items;
using Crm.Domain.Quotes.ValueObjects;

namespace Crm.Application.Features.Quotes.Commands.ImportQuotes;

/// <summary>
/// Turns a calculator КП into a Brevi one (R-25). Prices stay as the calculator saved them; items are linked to the
/// current Catalog and Reference rows by code. What cannot be linked is transferred as it is and named in a warning:
/// a missing model becomes a «Виріб з нуля» without composition, a missing PPE a one-off PPE.
/// </summary>
public sealed class QuoteImportMapper(
    QuoteImportReferences references,
    IReadOnlyList<QuoteStaffMember> staff,
    IReadOnlyDictionary<string, long> customers,
    QuotePerson importer)
{
    public const string MissingCompositionReason = "Склад не перенесено з калькулятора.";

    private readonly Dictionary<string, QuoteStaffMember> _staff = staff
        .Where(x => !string.IsNullOrWhiteSpace(x.Email))
        .GroupBy(x => x.Email!.Trim().ToLowerInvariant())
        .ToDictionary(x => x.Key, x => x.First());

    private readonly Dictionary<string, QuoteImportPerson> _unknown = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<QuoteImportPerson> UnknownPeople => _unknown.Values.ToArray();

    public DomainResult<Quote> Map(ImportQuoteInput input, QuoteNumber number, string note, DateTimeOffset now,
        List<QuoteImportWarning> warnings)
    {
        var shown = number.ToString();
        var author = Person(input.Author) ?? new QuotePerson(importer.UserId, input.Author.Name.Trim());
        var responsible = Person(input.Responsible) ?? importer;
        var kind = Enum.Parse<QuoteKind>(input.Kind);

        long? customerId = null;
        if (!string.IsNullOrWhiteSpace(input.KeepinClientId))
        {
            if (customers.TryGetValue(input.KeepinClientId.Trim(), out var id)) customerId = id;
            else warnings.Add(new(shown, null, $"Клієнта KeepinCRM {input.KeepinClientId} немає в Brevi; КП без клієнта."));
        }

        var header = new QuoteHeader(input.InternalName, customerId, input.RecipientName, input.CompanyName,
            input.ClientComment, input.ManagerNote);
        var items = input.Items.Select((item, index) => Item(index, item, input.UpdatedAt, author, shown, warnings))
            .ToArray();
        var introduction = kind == QuoteKind.Introduction
            ? new QuoteIntroduction(input.Introduction is { } intro ? Enum.Parse<QuoteIntroLayout>(intro.Layout)
                : QuoteIntroLayout.General, input.Introduction?.RecipientEmail, null)
            : null;
        var events = (input.Events ?? []).OrderBy(x => x.At).Select(x => new QuoteImportedEvent(
            Enum.Parse<QuoteEventKind>(x.Kind), Person(x.Actor) ?? new QuotePerson(importer.UserId, x.Actor.Name.Trim()),
            x.Details, x.At));
        var sent = input.Status == nameof(QuoteStatus.Sent);
        var state = new QuoteImportedState(number, kind, author, responsible, input.CreatedAt,
            input.UpdatedAt < input.CreatedAt ? input.CreatedAt : input.UpdatedAt, sent, sent ? input.LastSentAt : null);
        return Quote.Import(state, header, items, introduction, events, importer, note, now);
    }

    private QuotePerson? Person(ImportPersonInput person)
    {
        var email = person.Email?.Trim().ToLowerInvariant();
        if (email is { Length: > 0 } && _staff.TryGetValue(email, out var member))
            return new QuotePerson(member.UserId, member.Name);
        _unknown.TryAdd(email ?? person.Name.Trim(), new QuoteImportPerson(email, person.Name.Trim()));
        return null;
    }

    private QuoteItem Item(int position, ImportItemInput input, DateTimeOffset takenAt, QuotePerson author,
        string number, List<QuoteImportWarning> warnings)
    {
        var kind = Enum.Parse<QuoteItemKind>(input.Kind);
        void Warn(string message) => warnings.Add(new(number, input.Name.Trim(), message));
        return kind switch
        {
            QuoteItemKind.Sewing => Sewing(position, input, takenAt, Warn),
            QuoteItemKind.Ppe => Ppe(position, input, takenAt, author, Warn),
            QuoteItemKind.CustomSewing => Custom(position, input, null, input.Lines ?? [], takenAt, Warn),
            _ => OneOff(position, input, author),
        };
    }

    private QuoteItem Sewing(int position, ImportItemInput input, DateTimeOffset takenAt, Action<string> warn)
    {
        var code = input.Code?.Trim() ?? "";
        if (!references.Sewing.TryGetValue(code, out var model))
        {
            warn($"Моделі {code} немає в каталозі: перенесено як виріб з нуля без складу.");
            return Custom(position, input, code, [], takenAt, warn, describedMissing: true);
        }

        var options = input.Options.Select((option, index) =>
        {
            var fabricId = Fabric(option.FabricCode);
            var variant = fabricId is null ? null
                : model.Variants.Where(x => x.FabricId == fabricId).OrderByDescending(x => x.IsActive).FirstOrDefault();
            if (variant is null)
                warn($"Варіанта «{option.Title.Trim()}» немає в каталозі: ціну збережено, «Перерахувати» її не оновить.");
            return Option(index, option, variant?.ArticleId, fabricId, takenAt);
        });
        return new QuoteItem(position, QuoteItemKind.Sewing, model.ProductId,
            Texts(input, code, QuoteItemsBuilder.SewingSubtitle), options);
    }

    private QuoteItem Ppe(int position, ImportItemInput input, DateTimeOffset takenAt, QuotePerson author,
        Action<string> warn)
    {
        var code = input.Code?.Trim() ?? "";
        if (!references.PpeByCode.TryGetValue(code, out var productId) &&
            !references.PpeByName.TryGetValue(input.Name.Trim().ToLowerInvariant(), out productId))
        {
            warn($"Товару ЗІЗ {code} немає в каталозі: перенесено як разову позицію з тими самими цінами.");
            return OneOff(position, input, author);
        }

        var source = input.Options[0];
        var option = Option(0, source, null, null, takenAt, title: "Ціна", recommended: false);
        if (option.HasManualPrice)
            option.SetManualAudit(source.ManualReason ?? "Перенесено з калькулятора",
                Manual(source, author), source.ManualAt ?? takenAt);
        return new QuoteItem(position, QuoteItemKind.Ppe, productId,
            Texts(input, code, QuoteItemsBuilder.PpeSubtitle), [option]);
    }

    /// <summary>A one-off PPE has only manual prices: a calculator price becomes the manual one.</summary>
    private static QuoteItem OneOff(int position, ImportItemInput input, QuotePerson author)
    {
        var source = input.Options[0];
        static QuotePrice Price(ImportPriceInput price) => new(null, price.Manual ?? price.Source, price.Shown);
        var option = new QuoteItemOption(0, null, null, "Ціна", null, false, Price(source.Prices.Retail),
            Price(source.Prices.Wholesale), Price(source.Prices.Large), null);
        if (option.HasManualPrice) option.SetManualAudit(source.ManualReason, author, source.ManualAt);
        return new QuoteItem(position, QuoteItemKind.OneOffPpe, null,
            Texts(input, null, QuoteItemsBuilder.OneOffSubtitle) with { SiteUrl = null, Season = null }, [option]);
    }

    /// <summary>
    /// A «Виріб з нуля» with its composition by Reference codes. When a code is gone the composition is left out, so
    /// «Перерахувати» does not price an incomplete product.
    /// </summary>
    private QuoteItem Custom(int position, ImportItemInput input, string? code, IReadOnlyList<ImportLineInput> source,
        DateTimeOffset takenAt, Action<string> warn, bool describedMissing = false)
    {
        var lines = new List<QuoteItemLine>(source.Count);
        foreach (var line in source)
        {
            var kind = Enum.Parse<QuoteItemLineKind>(line.Kind);
            var table = kind switch
            {
                QuoteItemLineKind.Fabric => references.Fabrics,
                QuoteItemLineKind.Operation => references.Operations,
                _ => references.Accessories,
            };
            if (!table.TryGetValue(line.Code.Trim(), out var row))
            {
                warn($"У довіднику немає {line.Code.Trim()}: склад не перенесено, ціну збережено.");
                lines.Clear();
                break;
            }

            lines.Add(new QuoteItemLine(lines.Count, kind, row.Id, row.Name, row.Unit, line.Quantity, null));
        }

        var options = input.Options.Select((option, index) =>
        {
            var fabric = FabricRow(option.FabricCode);
            if (fabric is null && !describedMissing)
                warn($"Тканини {option.FabricCode} немає в довіднику: ціну збережено.");
            var row = Option(index, option, null, fabric?.Id, takenAt);
            return fabric is not null && option.FabricMeters is > 0
                ? row.WithFabrics([new QuoteItemOptionFabric(0, fabric.Id, fabric.Name, option.FabricMeters.Value)])
                : row;
        });
        return new QuoteItem(position, QuoteItemKind.CustomSewing, null,
            Texts(input, code, QuoteCustomSewingItemBuilder.Subtitle), options.ToArray(), lines);
    }

    private int? Fabric(string? code) => FabricRow(code)?.Id;

    private QuoteImportReference? FabricRow(string? code) =>
        code is { Length: > 0 } && references.Fabrics.TryGetValue(code.Trim(), out var fabric) ? fabric : null;

    private QuotePerson Manual(ImportOptionInput option, QuotePerson author) =>
        option.ManualByEmail is null && option.ManualByName is null
            ? author
            : Person(new ImportPersonInput(option.ManualByEmail, option.ManualByName ?? author.Name))
              ?? new QuotePerson(importer.UserId, option.ManualByName ?? author.Name);

    private static QuoteItemOption Option(int position, ImportOptionInput input, long? articleId, int? fabricId,
        DateTimeOffset takenAt, string? title = null, bool? recommended = null) =>
        new(position, articleId, fabricId, title ?? input.Title, input.Details, recommended ?? input.IsRecommended,
            Price(input.Prices.Retail), Price(input.Prices.Wholesale), Price(input.Prices.Large), takenAt);

    private static QuotePrice Price(ImportPriceInput price) => new(price.Source, price.Manual, price.Shown);

    private static QuoteItemTexts Texts(ImportItemInput input, string? code, string subtitle) =>
        new(code, input.Name, subtitle, input.Description, input.SiteUrl, Https(input.PhotoUrl), input.Season,
            input.SupplierUrl);

    private static string? Https(string? url) =>
        url is { Length: > 0 } && url.Trim().StartsWith("https://", StringComparison.OrdinalIgnoreCase) ? url : null;
}
