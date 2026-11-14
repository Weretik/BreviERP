using Crm.Infrastructure.Persistence;
using Ardalis.Specification.EntityFrameworkCore;
using Crm.Domain.Customers;
using Crm.Application.Abstractions.Customers;
namespace Crm.Infrastructure.Persistence.Customers;

public sealed class CustomerReadRepository(CrmDbContext db) : RepositoryBase<Customer>(db), ICustomerReadRepository;
