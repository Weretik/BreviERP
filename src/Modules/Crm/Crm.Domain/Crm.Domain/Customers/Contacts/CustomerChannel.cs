using System.Net.Mail;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Results;
using Crm.Domain.Customers.Identifiers;

namespace Crm.Domain.Customers.Contacts;

public sealed class CustomerChannel : BaseEntity<CustomerChannelId>
{
    public CustomerContactId ContactId { get; private set; }
    public string Kind { get; private set; } = null!;
    public string Value { get; private set; } = null!;
    public string NormalizedValue { get; private set; } = null!;

    private CustomerChannel() { }

    public static DomainResult<CustomerChannel> Create(string kind, string value)
    {
        if (kind is not ("email" or "phone")) return CustomerErrors.ChannelKindInvalid();
        var required = CustomerText.Required(value, kind == "email" ? 254 : 80, "channels");
        if (required.IsFailure) return required.Error!;
        var text = required.Value;
        if (kind == "email" && (!MailAddress.TryCreate(text, out var email) || email.Address != text))
            return CustomerErrors.EmailInvalid();
        var normalized = kind == "email" ? CustomerText.Normalize(text) : new string(text.Where(char.IsDigit).ToArray());
        if (kind == "phone" && (normalized.Length < 5 || normalized.Length > 20)) return CustomerErrors.PhoneInvalid();
        return new CustomerChannel { Kind = kind, Value = text, NormalizedValue = normalized };
    }
}
