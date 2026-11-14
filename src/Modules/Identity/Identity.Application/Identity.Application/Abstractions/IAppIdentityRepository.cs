namespace Identity.Application.Abstractions;

public interface IAppIdentityRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot
{

}
