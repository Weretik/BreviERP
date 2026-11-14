using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
namespace Catalog.Application.Abstractions.Pricing;
public sealed record PpeRuleValues(string Code, PpeMarkups Markups, string Source, string SourceDate);
