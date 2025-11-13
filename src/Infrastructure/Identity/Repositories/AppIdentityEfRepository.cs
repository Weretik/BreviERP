using Application.Identity.Interfaces;

namespace Infrastructure.Identity.Repositories;

public class AppIdentityEfRepository<T>(IdentityDbContext dbContext)
    : RepositoryBase<T>(dbContext), IAppIdentityReadRepository<T>, IAppIdentityRepository<T>
    where T : class, IAggregateRoot;
