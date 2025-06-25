using Microsoft.EntityFrameworkCore;
using RegistryApi.Data;
using RegistryApi.Models;
using RegistryApi.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

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

        public async Task<IEnumerable<UserGroupDto>> GetAllWithCountsAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Select(g => new UserGroupDto(
                    g.UserGroupID,
                    g.GroupName,
                    g.Description,
                    g.CreatedDate,
                    g.Users.Count(),
                    g.Specifications.Count(),
                    g.Specifications.Count(s => s.RegistrationStatus != null && s.RegistrationStatus.ToLower() == "in progress"),
                    g.Specifications.Count(s => s.RegistrationStatus != null && s.RegistrationStatus.ToLower() == "submitted"),
                    g.Specifications.Count(s => s.RegistrationStatus != null && s.RegistrationStatus.ToLower() == "under review"),
                    g.Specifications.Count(s => s.RegistrationStatus != null && s.RegistrationStatus.ToLower() == "verified")
                ))
                .ToListAsync();
        }
    }
}

