using Crm.Domain.Customers;

namespace Crm.Application.Customers.Access;

public sealed record CrmActor(int UserId, bool IsAdmin)
{
    public bool CanAccess(Customer customer) => UserId > 0 && (IsAdmin || customer.OwnerId.Value == UserId);
}
