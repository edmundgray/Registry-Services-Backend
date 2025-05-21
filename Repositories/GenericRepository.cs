using Microsoft.EntityFrameworkCore;
using RegistryApi.Data;

namespace RegistryApi.Repositories;

// Using primary constructor
public class GenericRepository<T>(RegistryDbContext context) : IGenericRepository<T> where T : class
{
    protected readonly RegistryDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        // FindAsync is optimized for finding by primary key
        return await _dbSet.FindAsync(id);
    }

     public virtual void Update(T entity)
    {
         // Using Update is simpler than managing state manually
         _dbSet.Update(entity);
    }

    public async Task<bool> SaveChangesAsync()
    {
        // SaveChangesAsync returns the number of state entries written to the database.
        return (await _context.SaveChangesAsync()) > 0;
    }
}
