namespace Crm.Application.Leads.Sheet;

/// <summary>Reads the leads tab of «Лиды Brevi» as shown (formatted values), the header row first.</summary>
public interface ILeadSheetReader
{
    /// <exception cref="LeadSheetUnavailableException">Google cannot be reached or refuses access.</exception>
    /// <exception cref="LeadSheetFormatException">The tab is missing or too big.</exception>
    Task<IReadOnlyList<IReadOnlyList<string?>>> ReadAsync(CancellationToken cancellationToken);
}

/// <summary>The sheet cannot be read now (network, access, Google answer): 503, nothing changes.</summary>
public sealed class LeadSheetUnavailableException(string message, Exception? inner = null) : Exception(message, inner);

/// <summary>The sheet breaks a rule (no ID column, the same ID twice): 400, nothing changes.</summary>
public sealed class LeadSheetFormatException(string message) : Exception(message);
