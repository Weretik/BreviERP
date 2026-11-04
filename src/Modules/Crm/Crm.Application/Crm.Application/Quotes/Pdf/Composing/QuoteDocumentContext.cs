using Crm.Application.Quotes.Settings.Contracts;
using Crm.Application.Quotes.Staff;
using Crm.Domain.Quotes.Settings;

namespace Crm.Application.Quotes.Pdf.Composing;

/// <summary>The settings a КП document is built with and the client-facing contact of one staff member.</summary>
public sealed record QuoteDocumentContext(QuoteSettingsContent Settings, QuoteSettings? Saved)
{
    public QuotePdfContact ContactOf(QuoteStaffMember member)
    {
        var contact = QuoteSettingsMapper.ContactOrAccount(member, Saved?.ContactOf(member.UserId));
        return new QuotePdfContact(contact.Name, contact.Phone, contact.Email);
    }
}
