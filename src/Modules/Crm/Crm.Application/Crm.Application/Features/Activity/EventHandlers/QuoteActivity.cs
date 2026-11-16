using ActivityLog.Contracts.Activity;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Enums;
using Crm.Domain.Quotes.ValueObjects;

namespace Crm.Application.Features.Activity.EventHandlers;

/// <summary>How КП actions read in «Журнал дій» (ACT-001): created, copied, changed fields, sent, deleted.</summary>
public sealed class QuoteActivity : IActivityDescriber
{
    public const string Subject = "quote";
    public const string SubjectTitle = "КП";
    public const string TemplateSubject = "quote-intro-template";
    public const string SettingsSubject = "quote-settings";

    public ActivityDescription? Describe(object domainEvent) => domainEvent switch
    {
        QuoteRecorded recorded => Recorded(recorded),
        QuoteDeleted deleted => Of(ActivityActions.Deleted, deleted.QuoteId, deleted.Number, deleted.InternalName, null, []),
        QuoteIntroTemplateSaved saved => Template(ActivityActions.Created, saved.Template.Id, saved.Template.Name),
        QuoteIntroTemplateDeleted deleted => Template(ActivityActions.Deleted, deleted.TemplateId, deleted.Name),
        QuoteSettingsChanged changed => new ActivityDescription(ActivityActions.Changed, SettingsSubject,
            "Налаштування КП", "settings", "Налаштування КП",
            "Збережено версію " + changed.Version.ToString(CultureInfo.InvariantCulture), [], "Crm"),
        _ => null,
    };

    private static ActivityDescription? Recorded(QuoteRecorded e)
    {
        var quote = e.Quote;
        return e.Kind switch
        {
            QuoteEventKind.Created => Of(ActivityActions.Created, quote.Id, quote.Number, quote.InternalName,
                quote.Kind == QuoteKind.Introduction ? "КП для знайомства" : null, []),
            QuoteEventKind.Copied => Of(ActivityActions.Created, quote.Id, quote.Number, quote.InternalName, e.Details, []),
            QuoteEventKind.Saved => Changes(e.Before, e.After) is { Any: true } changes
                ? Of(ActivityActions.Changed, quote.Id, quote.Number, quote.InternalName, null, changes.Items)
                : null,
            QuoteEventKind.Sent => Of(ActivityActions.Sent, quote.Id, quote.Number, quote.InternalName, e.Details, []),
            QuoteEventKind.Repriced or QuoteEventKind.EmbroideryAdded => Of(ActivityActions.Changed, quote.Id,
                quote.Number, quote.InternalName, Note(e), Changes(e.Before, e.After).Items),
            QuoteEventKind.ResponsibleChanged => Of(ActivityActions.Changed, quote.Id, quote.Number, quote.InternalName,
                "Відповідальний: " + e.Details, []),
            QuoteEventKind.Restored => Of(ActivityActions.Restored, quote.Id, quote.Number, quote.InternalName,
                "Повернуто з архіву", []),
            _ => null,
        };
    }

    private static string? Note(QuoteRecorded e) => e.Kind == QuoteEventKind.EmbroideryAdded
        ? "Додано вишивку" + (string.IsNullOrWhiteSpace(e.Details) ? "" : ": " + e.Details)
        : e.Details;

    private static ActivityChanges Changes(QuoteSnapshot? before, QuoteSnapshot after)
    {
        var changes = new ActivityChanges();
        if (before is null) return changes;
        return changes
            .Add("Назва", before.InternalName, after.InternalName)
            .Add("Клієнт", Customer(before.CustomerId), Customer(after.CustomerId))
            .Add("Отримувач", before.RecipientName, after.RecipientName)
            .Add("Компанія", before.CompanyName, after.CompanyName)
            .Add("Коментар для клієнта", before.ClientComment, after.ClientComment)
            .Add("Нотатка менеджера", before.ManagerNote, after.ManagerNote)
            .Add("Позицій", before.ItemCount, after.ItemCount)
            .Add("Сторінки", Layout(before.IntroLayout), Layout(after.IntroLayout))
            .Add("E-mail отримувача", before.RecipientEmail, after.RecipientEmail)
            .Add("Варіант листа", before.LetterName, after.LetterName)
            .Add("Тема листа", before.LetterSubject, after.LetterSubject)
            .Add("Текст листа", LetterBody(before.LetterBody, after.LetterBody, "попередній"),
                LetterBody(after.LetterBody, before.LetterBody, "новий"));
    }

    /// <summary>A long letter is not copied into the journal: only that it was written, changed or cleared.</summary>
    private static string? LetterBody(string? value, string? other, string word) =>
        string.Equals(value, other, StringComparison.Ordinal) || value is null ? null : word + " текст";

    private static string? Customer(long? id) => id is { } value ? $"клієнт #{value}" : null;

    private static string? Layout(QuoteIntroLayout? layout) => layout switch
    {
        QuoteIntroLayout.General => "Літо і зима",
        QuoteIntroLayout.Summer => "Літо",
        QuoteIntroLayout.Winter => "Зима",
        _ => null,
    };

    private static ActivityDescription Template(string action, long id, string name) =>
        new(action, TemplateSubject, "Шаблон КП для знайомства", id.ToString(CultureInfo.InvariantCulture), name, null,
            [], "Crm");

    private static ActivityDescription Of(string action, long id, string number, string name, string? summary,
        IReadOnlyList<ActivityChangeItem> changes) =>
        new(action, Subject, SubjectTitle, id.ToString(CultureInfo.InvariantCulture), $"{number} · {name}", summary,
            changes, "Crm");
}
