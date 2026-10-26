using Ardalis.Specification;
using Crm.Domain.Customers;

namespace Crm.Application.Customers.Persistence;

public interface ICustomerReadRepository : IReadRepositoryBase<Customer>;
