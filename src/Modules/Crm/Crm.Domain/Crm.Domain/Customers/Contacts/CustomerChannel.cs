using System.Net.Mail;
using BuildingBlocks.Domain.Entity;
using Crm.Domain.Customers.Identifiers;

namespace Crm.Domain.Customers.Contacts;

public sealed class CustomerChannel : BaseEntity<CustomerChannelId>
{
    public CustomerContactId ContactId { get; private set; }
    public string Kind { get; private set; } = null!;
    public string Value { get; private set; } = null!;
    public string NormalizedValue { get; private set; } = null!;

    private CustomerChannel() { }

    public static CustomerChannel Create(string kind, string value)
    {
        if (kind is not ("email" or "phone")) throw new CustomerValidationException("channel.kind: invalid");
        var text = CustomerText.Required(value, kind == "email" ? 254 : 80, "channel.value");
        if (kind == "email" && (!MailAddress.TryCreate(text, out var email) || email.Address != text))
            throw new CustomerValidationException("channel.email: invalid");
        var normalized = kind == "email" ? CustomerText.Normalize(text) : new string(text.Where(char.IsDigit).ToArray());
        if (kind == "phone" && (normalized.Length < 5 || normalized.Length > 20))
            throw new CustomerValidationException("channel.phone: invalid");
        return new CustomerChannel { Kind = kind, Value = text, NormalizedValue = normalized };
    }
}
