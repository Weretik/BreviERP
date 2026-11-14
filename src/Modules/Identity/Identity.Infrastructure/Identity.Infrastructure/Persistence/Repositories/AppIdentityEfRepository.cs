using Identity.Application.Abstractions;
using BuildingBlocks.Domain.Entity;


namespace Identity.Infrastructure.Persistence.Repositories;

public class AppIdentityEfRepository<T>
    : RepositoryBase<T>, IAppIdentityReadRepository<T>, IAppIdentityRepository<T>
    where T : class, IAggregateRoot
{
    public AppIdentityEfRepository(IdentityDbContext dbContext)
        : base(dbContext)
    {
    }
}
