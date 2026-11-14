using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;

namespace Crm.Application.Customers.KeepinCrmSync;

/// <summary>What a KeepinCRM client becomes in Brevi: a company is an organization, a client without one is a person.</summary>
internal sealed record KeepinCrmClientTarget(
    string Kind,
    string Name,
    string ContactName,
    bool Active,
    IReadOnlyList<string> Phones,
    IReadOnlyList<string> Emails)
{
    private const int NameMax = 300;
    private const int ChannelsMax = 10;

    public static KeepinCrmClientTarget From(KeepinCrmClientSnapshot snapshot)
    {
        var company = Clean(snapshot.Company);
        var person = Clean(snapshot.Person);
        var name = company ?? person ?? $"Клієнт KeepinCRM {snapshot.KeepinCrmId}";
        return new KeepinCrmClientTarget(
            company is null ? "person" : "organization",
            Cut(name),
            Cut(person ?? name),
            !snapshot.Archived,
            snapshot.Phones,
            snapshot.Emails);
    }

    /// <summary>Valid, distinct phones then e-mails, at most 10; KeepinCRM values Brevi cannot accept are skipped.</summary>
    public IReadOnlyList<CustomerChannel> Channels()
    {
        var result = new List<CustomerChannel>();
        var keys = new HashSet<string>();
        foreach (var (kind, value) in Phones.Select(x => ("phone", x)).Concat(Emails.Select(x => ("email", x))))
        {
            if (result.Count == ChannelsMax) break;
            var created = CustomerChannel.Create(kind, value);
            if (created.IsFailure) continue;
            var channel = created.Value;
            if (keys.Add($"{channel.Kind}:{channel.NormalizedValue}"))
                result.Add(channel);
        }

        return result;
    }

    private static string? Clean(string? value)
    {
        var text = value?.Trim();
        return string.IsNullOrEmpty(text) ? null : text;
    }

    private static string Cut(string value) => value.Length <= NameMax ? value : value[..NameMax].TrimEnd();
}
