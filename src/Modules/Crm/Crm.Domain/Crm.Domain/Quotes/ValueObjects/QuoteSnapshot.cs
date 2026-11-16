using Crm.Domain.Quotes.Enums;

namespace Crm.Domain.Quotes.ValueObjects;

/// <summary>The fields of a КП a person sees above the items, the item count and the introduction (ACT-001 R-03).</summary>
public sealed record QuoteSnapshot(
    string InternalName,
    long? CustomerId,
    string? RecipientName,
    string? CompanyName,
    string? ClientComment,
    string? ManagerNote,
    int ItemCount,
    QuoteIntroLayout? IntroLayout,
    string? RecipientEmail,
    string? LetterName,
    string? LetterSubject,
    string? LetterBody);
