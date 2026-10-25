using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
namespace Catalog.Application.Contracts.Pricing.Ppe;
public sealed record PpeRuleValues(string Code, PpeMarkups Markups, string Source, string SourceDate);
