using System.Text.Json.Serialization;

namespace Accounting.Domain.Accounts;

/// <summary>Where the money is kept: «Банк», «Картка», «Каса», «Інше».</summary>
[JsonConverter(typeof(JsonStringEnumConverter<FinanceAccountType>))]
public enum FinanceAccountType
{
    Bank,
    Card,
    Cash,
    Other,
}
