namespace Purchasing.Domain.Messages;

/// <summary>The fields of a message taken from the sheet; equal for every row of the same Telegram message.</summary>
public sealed record MessageSource(
    string? MessageAt,
    DateTime? MessageDate,
    string Text,
    string StatusOriginal,
    string? MarkAt,
    string? MarkBy,
    PaymentState PaymentState);
