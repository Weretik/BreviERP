using System.Text.Json.Serialization;

namespace Accounting.Domain.Transactions;

/// <summary>
/// Whether an operation needs a look («Потребують перевірки»): a bank line without a category, or one with an unchecked
/// finance counterparty. It only tells the quality of the classification: such operations still count in balances.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<VerificationStatus>))]
public enum VerificationStatus
{
    Verified,
    NeedsCategory,
    NeedsCounterpartyReview,
}
