using IdentityService.Domain.Entities.Interfaces;
using IdentityService.Domain.Interfaces.Infraestructure;

namespace IdentityService.Domain.Interfaces;

public interface IBaseService<T>  : IRepository<T> where T : class, IBaseEntity
{
    Task Remove(T entity);
}
