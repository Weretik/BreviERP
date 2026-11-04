using Crm.Application.Quotes.Pdf;

namespace Crm.Application.Quotes.Sending;

/// <summary>Fills {quoteNumber}, {managerName}, {managerPhone}, {managerEmail} and {site} in letter texts.</summary>
internal static class QuoteMarks
{
    public static string Fill(string template, QuotePdfContact manager, string? site, string quoteNumber) =>
        QuotePdfModelBuilder.FillMarks(template, manager, site, quoteNumber) ?? "";
}
