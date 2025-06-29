using Microsoft.EntityFrameworkCore;
using RegistryApi.Data;
using RegistryApi.Models;
using System.Threading.Tasks;

namespace RegistryApi.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(RegistryDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetUsersByGroupIdAsync(int groupId)
        {
            return await _dbSet
                .Where(u => u.UserGroupID == groupId)
                .Include(u => u.UserGroup)
                .AsNoTracking()
                .ToListAsync();
        }

        // Example implementation for other methods:
        // public async Task<IEnumerable<User>> GetActiveUsersAsync()
        // {
        //     return await _dbSet.Where(u => u.IsActive).ToListAsync();
        // }
    }
}
