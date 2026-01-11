using System.Linq.Expressions;
using IdentityService.Domain.Entities.Interfaces;
using IdentityService.Domain.Interfaces.Infraestructure;

namespace IdentityService.Infrastructure.Data.Repositories;

public abstract class BaseRepository<T>(ApplicationDbContext context) : BaseExpressionService<T>(context), IRepository<T> where T : class, IBaseEntity
{
    public abstract Task<T> GetById(Guid id, bool include, bool tracking);

    public virtual async Task<T> Add(T entity)
    {
        await Context.Set<T>().AddAsync(entity);
        await Context.SaveChangesAsync();

        return entity;
    }

    public virtual async Task<T> Update(T entity)
    {
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync();

        return entity;
    }

    public virtual async Task Delete(Guid id)
    {
        Context.Remove(id);
        await Context.SaveChangesAsync();
    }

    public virtual IEnumerable<T> GetAll()
    {
        return Context.Set<T>().Where(x => x.Removed == false).ToList();
    }

    public async Task<IEnumerable<T>> AddRange(IEnumerable<T> entities)
    {
        Context.Set<T>().AddRange(entities);
        await Context.SaveChangesAsync();

        return entities;
    }

    public virtual IQueryable<T> FindBy(Expression<Func<T, bool>> expression)
    {
        return Context.Set<T>().Where(expression).Where(x => x.Removed == false).AsQueryable();
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
