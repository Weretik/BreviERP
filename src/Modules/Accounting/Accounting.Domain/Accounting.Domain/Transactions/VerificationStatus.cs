using System.Text.Json.Serialization;

namespace Accounting.Domain.Transactions;

/// <summary>
/// Whether an operation needs a look: a bank line without a category, or (stage 3) one with an unchecked counterparty.
/// Hand-made operations always have a category, so they are <see cref="Verified"/> until counterparties arrive.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<VerificationStatus>))]
public enum VerificationStatus
{
    Verified,
    NeedsCategory,
    NeedsCounterpartyReview,
}
