using Identity.Application.Abstractions;

namespace Identity.Infrastructure.Persistence.Users;

public class AppIdentityEfRepository<T>
    : RepositoryBase<T>, IAppIdentityReadRepository<T>, IAppIdentityRepository<T>
    where T : class, IAggregateRoot
{
    public AppIdentityEfRepository(IdentityDbContext dbContext)
        : base(dbContext)
    {
    }
}
