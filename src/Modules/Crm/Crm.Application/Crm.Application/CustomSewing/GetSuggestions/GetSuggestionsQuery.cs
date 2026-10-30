using Crm.Application.CustomSewing.GetSuggestions.Contracts;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Mediator;
namespace Crm.Application.CustomSewing.GetSuggestions;

// ProductType is one of the six exact legacy garment groups; summer/winter maps to Літо/Зима.
public sealed record GetSuggestionsQuery(string ProductType, string Season, CrmActor Actor) : IQuery<Result<SewingSuggestions>>;
