using Crm.Application.Features.Quotes.Shared.Pdf;

namespace Crm.Application.Features.Quotes.Shared.Sending;

/// <summary>
/// Fills {quoteNumber}, {managerName}, {managerPhone}, {managerEmail}, {site}, {clientName} and {companyName} in letter texts.
/// </summary>
internal static class QuoteMarks
{
    public static string Fill(string template, QuotePdfContact manager, string? site, string quoteNumber,
        string? clientName, string? companyName) =>
        QuotePdfModelBuilder.FillMarks(template, manager, site, quoteNumber, clientName, companyName) ?? "";
}
