using RegistryApi.Models;
using RegistryApi.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RegistryApi.Repositories
{
    public interface IUserGroupRepository : IGenericRepository<UserGroup>
    {
        Task<UserGroup?> GetByNameAsync(string groupName);
        Task<IEnumerable<UserGroupDto>> GetAllWithCountsAsync();
    }
}
