using RegistryApi.Models;
using System.Threading.Tasks;

namespace RegistryApi.Repositories
{
    public interface IUserGroupRepository : IGenericRepository<UserGroup>
    {
        Task<UserGroup?> GetByNameAsync(string groupName);
        // Add other group-specific query methods if needed
    }
}
