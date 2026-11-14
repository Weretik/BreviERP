using Crm.Domain.Customers;
using Crm.Domain.Customers.ExternalIdentities;

namespace Crm.Application.Customers.KeepinCrmSync;

/// <summary>The link from a Brevi customer to a KeepinCRM client: identity keepincrm / client / id.</summary>
public static class KeepinCrmClientIdentity
{
    public const string Source = "keepincrm";
    public const string Type = "client";

    /// <summary>Position of the contact that holds the KeepinCRM person and phones; other contacts stay as set in Brevi.</summary>
    public const string ContactPosition = "KeepinCRM";

    public static string? IdOf(Customer customer) =>
        customer.ExternalIdentities.FirstOrDefault(x => x.Source == Source && x.Type == Type)?.ExternalId;

    public static DomainResult<CustomerExternalIdentity> Create(string keepinCrmId) =>
        CustomerExternalIdentity.Create(Source, Type, keepinCrmId);
}
