using Crm.Application.Abstractions.Customers;

namespace Crm.Application.Features.Customers.Queries.GetCustomerInternalId;

public sealed class GetCustomerInternalIdHandler(ICustomerReadRepository repository)
    : IQueryHandler<GetCustomerInternalIdQuery, long?>
{
    public async ValueTask<long?> Handle(GetCustomerInternalIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FirstOrDefaultAsync(new CustomerInternalIdSpec(query.PublicId), cancellationToken);
    }
}
