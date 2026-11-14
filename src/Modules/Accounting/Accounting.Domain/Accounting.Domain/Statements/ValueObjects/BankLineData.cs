using Accounting.Domain.Categories;

namespace Accounting.Domain.Statements.ValueObjects;

/// <summary>
/// One row of a statement file as the bank gave it, already read: the values that could be read, the bank details
/// (empty strings when the file has none) and <see cref="Error"/> with the reasons when the row cannot be booked.
/// <see cref="RawJson"/> keeps the original cells by header name.
/// </summary>
public sealed record BankLineData(
    int RowNumber,
    DateOnly? Date,
    TimeOnly? Time,
    CategoryDirection? Type,
    long? AmountMinor,
    string? Currency,
    string BankName,
    string Edrpou,
    string Iban,
    string Purpose,
    string Document,
    string? BankId,
    string? Fingerprint,
    string Error,
    string RawJson)
{
    public const int NameMaxLength = 250;
    public const int CodeMaxLength = 64;
    public const int PurposeMaxLength = 1500;
    public const int DocumentMaxLength = 100;
    public const int BankIdMaxLength = 200;
    public const int ErrorMaxLength = 500;
    public const int RawJsonMaxLength = 20_000;

    public bool HasError => Error.Length > 0;

    /// <summary>Whether the bank named who the money is with, so a counterparty can be recognised or created.</summary>
    public bool HasParty => BankName.Length > 0 || Edrpou.Length > 0 || Iban.Length > 0;
}
