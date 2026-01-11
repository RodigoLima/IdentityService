using System.Linq.Expressions;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Entities.Interfaces;
using IdentityService.Domain.Interfaces;
using IdentityService.Domain.Interfaces.Infraestructure;

namespace IdentityService.Domain.Services;

public class BaseService<T>(IRepository<T> repository, UserData userData) : IBaseService<T> where T : class, IBaseEntity
{
    protected readonly IRepository<T> _repository = repository;
    protected readonly UserData _userData = userData;

    public virtual async Task<T> Add(T entity)
    {
        entity.PrepareToInsert(_userData.Id);
        return await _repository.Add(entity);
    }

    public async Task Delete(Guid id)
    {
        await _repository.Delete(id);
    }

    public async Task Remove(T entity)
    {
        entity.PrepareToRemove(_userData.Id);
        await _repository.Update(entity);
    }

    public IEnumerable<T> GetAll()
    {
        return _repository.GetAll();
    }

    public virtual async Task<T> Update(T entity)
    {
        entity.PrepareToUpdate(_userData.Id);
        return await _repository.Update(entity);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _repository.Dispose();
    }

    public IQueryable<T> FindBy(Expression<Func<T, bool>> expression)
    {
        return _repository.FindBy(expression);
    }
}
