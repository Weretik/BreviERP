using Ardalis.Specification;
using Crm.Domain.Customers;

namespace Crm.Application.Abstractions.Customers;

public interface ICustomerReadRepository : IReadRepositoryBase<Customer>;
