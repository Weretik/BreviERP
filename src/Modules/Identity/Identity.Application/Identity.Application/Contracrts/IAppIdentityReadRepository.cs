namespace Identity.Application.Contracrts;

public interface IAppIdentityReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot
{

}
