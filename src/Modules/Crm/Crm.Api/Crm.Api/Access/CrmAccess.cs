namespace Crm.Api.Access;

/// <summary>
/// «Права доступу» pages of the CRM. A policy without an action takes it from the HTTP method; the director always
/// passes. The host checks them against the rights saved for each role.
/// </summary>
internal static class CrmAccess
{
    /// <summary>«Комерційні пропозиції».</summary>
    public const string Quotes = "access:quotes";

    /// <summary>Lookups and the own mailbox while working on a КП: seeing КП is enough.</summary>
    public const string QuotesView = "access:quotes:view";

    /// <summary>Deleting a КП (only the Director until someone hands it out).</summary>
    public const string QuotesDelete = "access:quotes:delete";

    /// <summary>«Налаштування КП».</summary>
    public const string QuoteSettings = "access:quote-settings";

    /// <summary>«Ціна вишивки».</summary>
    public const string Embroidery = "access:embroidery";

    /// <summary>«Мої завдання».</summary>
    public const string Tasks = "access:tasks";

    /// <summary>«Ліди»: «Мій день» and «Усі ліди» (LEAD-001).</summary>
    public const string Leads = "access:leads";

    /// <summary>«Адміністрування › Ліди»: statistics, references; creating is «Синхронізувати зараз».</summary>
    public const string LeadStats = "access:lead-stats";

    /// <summary>«Синхронізувати зараз» (Director and Admin until someone hands it out).</summary>
    public const string LeadSync = "access:lead-stats:create";

    /// <summary>«Контрагенти»: clients and their contacts.</summary>
    public const string Counterparties = "access:counterparties";
}
