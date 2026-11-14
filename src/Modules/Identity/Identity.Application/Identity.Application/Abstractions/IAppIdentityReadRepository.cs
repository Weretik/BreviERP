using BuildingBlocks.Domain.Entity;

namespace Identity.Application.Abstractions;

public interface IAppIdentityReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot
{

}
