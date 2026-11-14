using Crm.Domain.Customers;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;

namespace Crm.Application.Features.Customers.Queries.GetCustomersByKeepinIds;

public sealed class GetCustomersByKeepinIdsValidator : AbstractValidator<GetCustomersByKeepinIdsQuery>
{
    public GetCustomersByKeepinIdsValidator()
    {
        RuleFor(x => x.KeepinIds).NotNull().Must(x => x.Count <= 5000);
        RuleForEach(x => x.KeepinIds).NotEmpty().MaximumLength(200);
    }
}
