using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
namespace Catalog.Application.Abstractions.Pricing.Ppe.Versions;
public sealed record PpeRuleValues(string Code, PpeMarkups Markups, string Source, string SourceDate)
{
    /// <summary>The rule values as the page sends them: three markups in percent.</summary>
    public static PpeRuleValues Of(string code, decimal retail, decimal wholesale, decimal largeWholesale, string source,
        string sourceDate) => new(code, new PpeMarkups(retail, wholesale, largeWholesale), source, sourceDate);
}
