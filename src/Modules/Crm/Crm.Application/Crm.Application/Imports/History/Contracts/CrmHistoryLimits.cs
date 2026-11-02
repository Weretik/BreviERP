namespace Crm.Application.Imports.History.Contracts;

public static class CrmHistoryLimits
{
    public const int MaximumRows = 100_000;
    public const int MaximumLedgerEntries = 200_000;
    public const int MaximumReferences = 1_000_000;
    public const int MaximumRowReferences = 256;
}
