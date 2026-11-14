using Crm.Application.Abstractions.Customers;
using Crm.Application.Abstractions.Customers.Views;
namespace Crm.Application.Features.Customers.Queries.GetCustomer;

public sealed class GetCustomerHandler(ICustomerReadRepository repository) : IQueryHandler<GetCustomerQuery, Result<CustomerDetail>>
{
    public async ValueTask<Result<CustomerDetail>> Handle(GetCustomerQuery query, CancellationToken cancellationToken)
    {
        if (query.Actor.UserId <= 0) return Result.Forbidden();
        var detail = await repository.FirstOrDefaultAsync(new CustomerDetailSpec(query.CustomerId, query.Actor), cancellationToken);
        return detail is null ? Result.NotFound() : Result.Success(detail);
    }
}
