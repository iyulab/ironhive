using Microsoft.EntityFrameworkCore;
using Raggle.Server.Web.Models;
using System.Linq.Expressions;

namespace Raggle.Server.Web.Database;

public class AppRepository<T> where T : BaseEntity
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public AppRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = false,
        int skip = 0,
        int limit = 0,
        bool asNoTracking = false)
    {
        IQueryable<T> query = _dbSet.Where(predicate);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        query = ApplyQueryOptions(query, orderBy, descending, skip, limit);

        return await query.ToListAsync();
    }

    public async Task<T?> FindFirstAsync(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = false,
        bool asNoTracking = false)
    {
        IQueryable<T> query = _dbSet.Where(predicate);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        query = ApplyQueryOptions(query, orderBy, descending);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync(bool asNoTracking = false)
    {
        IQueryable<T> query = _dbSet;

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id, bool asNoTracking = false)
    {
        IQueryable<T> query = _dbSet;

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "ID") == id);
    }

    public async Task<T> AddAsync(T entity)
    {
        // 동일한 ID 존재 확인
        while (await _dbSet.AnyAsync(e => e.ID == entity.ID))
        {
            entity.ID = Guid.NewGuid();
        }

        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id)
            ?? throw new KeyNotFoundException($"Entity with ID {id} not found");
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    private IQueryable<T> ApplyQueryOptions(
        IQueryable<T> query,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = false,
        int skip = 0,
        int limit = 0)
    {
        if (orderBy != null)
        {
            query = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
        }

        if (skip > 0)
        {
            query = query.Skip(skip);
        }

        if (limit > 0)
        {
            query = query.Take(limit);
        }

        return query;
    }
}
