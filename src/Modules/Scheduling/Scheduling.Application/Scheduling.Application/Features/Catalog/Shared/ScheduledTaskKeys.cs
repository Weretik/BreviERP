namespace Scheduling.Application.Features.Catalog.Shared;

/// <summary>Stable task keys: rows in the database and the names the job runs (--tasks).</summary>
public static class ScheduledTaskKeys
{
    public const string Outbox = "outbox";
    public const string Reminders = "reminders";
    public const string KeepinSuppliers = "keepin-suppliers";
    public const string KeepinClients = "keepin-clients";
    public const string SewingPrices = "sewing-prices";
    public const string MediaThumbnails = "media-thumbnails";
    public const string PurchaseInvoices = "purchase-invoices";
    public const string Leads = "leads";
    public const string PpeSupplierPrices = "ppe-supplier-prices";
}
