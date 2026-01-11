using System.Linq.Expressions;
using IdentityService.Domain.Entities.Interfaces;

namespace IdentityService.Domain.Interfaces.Infraestructure;

public interface IRepository<T> : IDisposable where T : class, IBaseEntity
{
    IEnumerable<T> GetAll();
    Task<T> Add(T entity);
    Task<T> Update(T entity);
    Task Delete(Guid id);
    IQueryable<T> FindBy(Expression<Func<T, bool>> expression);
}
