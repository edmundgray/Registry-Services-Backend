using Microsoft.EntityFrameworkCore;
using RegistryApi.Data;
using RegistryApi.Models;
using System.Threading.Tasks;

namespace RegistryApi.Repositories
{
    public class UserGroupRepository : GenericRepository<UserGroup>, IUserGroupRepository
    {
        public UserGroupRepository(RegistryDbContext context) : base(context)
        {
        }

        public async Task<UserGroup?> GetByNameAsync(string groupName)
        {
            return await _dbSet.FirstOrDefaultAsync(g => g.GroupName == groupName);
        }
    }
}

