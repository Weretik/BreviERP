using Crm.Infrastructure.Persistence;
using Ardalis.Specification.EntityFrameworkCore;
using Crm.Application.Customers.Persistence;
using Crm.Domain.Customers;
namespace Crm.Infrastructure.Persistence.Customers.Reading;

public sealed class CustomerReadRepository(CrmDbContext db) : RepositoryBase<Customer>(db), ICustomerReadRepository;
