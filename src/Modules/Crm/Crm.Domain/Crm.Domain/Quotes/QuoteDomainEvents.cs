using Crm.Domain.Quotes.Introductions;
using Crm.Domain.Quotes.ValueObjects;

namespace Crm.Domain.Quotes;

/// <summary>
/// A person added a history line to a КП: created, copied, saved, sent, repriced, responsible changed, restored or
/// embroidery added (ADR-0016). It carries the КП, whose database id is known only after the insert, and the fields
/// before and after the change.
/// </summary>
public sealed record QuoteRecorded(
    Quote Quote,
    QuoteEventKind Kind,
    string? Details,
    QuoteSnapshot? Before,
    QuoteSnapshot After) : IDomainEvent;

/// <summary>A КП was deleted.</summary>
public sealed record QuoteDeleted(long QuoteId, string Number, string InternalName) : IDomainEvent;

/// <summary>A «Готовий шаблон» of a «КП для знайомства» was saved (ACT-001).</summary>
public sealed record QuoteIntroTemplateSaved(QuoteIntroTemplate Template) : IDomainEvent;

/// <summary>A «Готовий шаблон» was deleted (ACT-001).</summary>
public sealed record QuoteIntroTemplateDeleted(long TemplateId, string Name) : IDomainEvent;

/// <summary>«Налаштування КП» were saved (ACT-001); <paramref name="Version"/> is the new version.</summary>
public sealed record QuoteSettingsChanged(int Version) : IDomainEvent;
