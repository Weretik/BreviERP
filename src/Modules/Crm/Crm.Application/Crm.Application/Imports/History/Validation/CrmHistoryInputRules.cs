using Crm.Application.Imports.History.Contracts;

namespace Crm.Application.Imports.History.Validation;

internal static class CrmHistoryInputRules
{
    internal const int MaximumRows = CrmHistoryLimits.MaximumRows;
    internal const int MaximumLedgerEntries = CrmHistoryLimits.MaximumLedgerEntries;
    internal const int MaximumReferences = CrmHistoryLimits.MaximumReferences;
    internal static readonly HashSet<string> Entities = new(StringComparer.Ordinal)
    {
        "counterparties", "quotes", "quote_versions", "quote_items", "quote_change_log", "quote_transfers",
        "quote_email_sends", "quote_ppe_price_overrides", "custom_sewing_drafts", "custom_sewing_versions",
        "custom_sewing_files", "introduction_saved_templates"
    };
    internal static bool Identifier(string? value, int maximum = 128) => value is { Length: > 0 } &&
        value.Length <= maximum && value.All(c => char.IsAsciiLetterOrDigit(c) || c is '_' or '-');
    internal static bool Hash(string? value) => value is { Length: 64 } && value.All(char.IsAsciiHexDigit);
    internal static bool BoundedText(string? value, int maximum) => value is not null && value.Length <= maximum &&
        value.All(c => !char.IsControl(c));
    internal static bool OptionalHash(string? value) => value is null || Hash(value);
}
