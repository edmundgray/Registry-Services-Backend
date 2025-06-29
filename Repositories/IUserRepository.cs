using RegistryApi.Models;
using System.Threading.Tasks;

namespace RegistryApi.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersByGroupIdAsync(int groupId);
        // Add other user-specific query methods if needed, for example:
        // Task<IEnumerable<User>> GetActiveUsersAsync();
    }
}

