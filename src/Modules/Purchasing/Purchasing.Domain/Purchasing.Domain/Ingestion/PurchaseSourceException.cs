namespace Purchasing.Domain.Ingestion;

/// <summary>The source sheet breaks a rule of the register; the sync stops and changes nothing.</summary>
public sealed class PurchaseSourceException(string message) : Exception(message);
