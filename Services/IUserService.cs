using RegistryApi.DTOs;
using RegistryApi.Helpers;
using RegistryApi.Models; // Assuming ServiceResult is here or in a shared DTOs/Helpers namespace
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RegistryApi.Services
{
    public interface IUserService
    {
        Task<(ServiceResult Status, UserDto? Dto, string? ErrorMessage)> RegisterUserAsync(UserCreateDto createDto);
        Task<(ServiceResult Status, UserDto? Dto)> UpdateUserAsync(int userId, UserUpdateDto updateDto); // Typically Admin action
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<UserDto?> GetUserByUsernameAsync(string username);
        Task<IEnumerable<UserDto>> GetAllUsersAsync(PaginationParams paginationParams); // Admin action
        Task<ServiceResult> AssignUserToGroupAsync(int userId, int? userGroupId); // Admin action
        Task<ServiceResult> ChangeUserRoleAsync(int userId, string newRole); // Admin action
        Task<(ServiceResult Status, UserTokenDto? TokenDto, string? ErrorMessage)> AuthenticateUserAsync(UserLoginDto loginDto);
        Task<ServiceResult> UpdateLastLoginDateAsync(int userId);
        Task<ServiceResult> DeleteUserAsync(int userId); // Admin action
    }
}