using RegistryApi.DTOs;
using RegistryApi.Helpers;
using RegistryApi.Models; // Assuming ServiceResult is here
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RegistryApi.Services
{
    public interface IUserGroupService
    {
        Task<(ServiceResult Status, UserGroupDto? Dto)> CreateUserGroupAsync(UserGroupCreateDto createDto);
        Task<ServiceResult> UpdateUserGroupAsync(int groupId, UserGroupUpdateDto updateDto);
        Task<ServiceResult> DeleteUserGroupAsync(int groupId); // Consider rules for deletion
        Task<UserGroupDto?> GetUserGroupByIdAsync(int groupId);
        Task<IEnumerable<UserGroupDto>> GetAllUserGroupsAsync(PaginationParams paginationParams);
    }
}
