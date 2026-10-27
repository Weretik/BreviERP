using Crm.Application.Customers.Contracts.Responses;
using Crm.Application.Customers.Persistence;
using Crm.Application.Customers.GetCustomer.Specifications;
namespace Crm.Application.Customers.GetCustomer;

public sealed class GetCustomerQueryHandler(ICustomerReadRepository repository) : IQueryHandler<GetCustomerQuery, Result<CustomerDetail>>
{
    public async ValueTask<Result<CustomerDetail>> Handle(GetCustomerQuery query, CancellationToken cancellationToken)
    {
        if (query.Actor.UserId <= 0) return Result.Forbidden();
        var detail = await repository.FirstOrDefaultAsync(new CustomerDetailSpec(query.CustomerId, query.Actor), cancellationToken);
        return detail is null ? Result.NotFound() : Result.Success(detail);
    }
}
