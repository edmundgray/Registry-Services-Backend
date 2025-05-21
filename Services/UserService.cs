using AutoMapper;
using RegistryApi.DTOs;
using RegistryApi.Models;
using RegistryApi.Repositories;
using RegistryApi.Helpers; // For PagedList
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // For logging

namespace RegistryApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserGroupRepository _userGroupRepository; // For validating UserGroupID
        private readonly IMapper _mapper;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IUserGroupRepository userGroupRepository,
            IMapper mapper,
            IPasswordHasher passwordHasher,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _userGroupRepository = userGroupRepository;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<(ServiceResult Status, UserDto? Dto, string? ErrorMessage)> RegisterUserAsync(UserCreateDto createDto)
        {
            if (await _userRepository.GetByUsernameAsync(createDto.Username) != null)
            {
                return (ServiceResult.Conflict, null, "Username already exists.");
            }
            if (await _userRepository.GetByEmailAsync(createDto.Email) != null)
            {
                return (ServiceResult.Conflict, null, "Email already exists.");
            }

            if (createDto.UserGroupID.HasValue && await _userGroupRepository.GetByIdAsync(createDto.UserGroupID.Value) == null)
            {
                return (ServiceResult.RefNotFound, null, "Specified UserGroup does not exist.");
            }


            var user = _mapper.Map<User>(createDto);
            user.PasswordHash = _passwordHasher.HashPassword(createDto.Password);
            // Role, CreatedDate, IsActive are set by AutoMapper profile or defaults in model

            // Per plan: New users get "User" role and null UserGroupID unless specified (and admin is creating)
            // For now, UserCreateDto has Role and UserGroupID. We'll assume an Admin might use this.
            // If public registration, Role would be fixed to "User" and UserGroupID to null.
            if (string.IsNullOrWhiteSpace(user.Role)) // Default role if not provided
            {
                user.Role = "User";
            }
            user.CreatedDate = DateTime.UtcNow;
            user.IsActive = true;


            await _userRepository.AddAsync(user);
            if (!await _userRepository.SaveChangesAsync())
            {
                _logger.LogError("Failed to save new user during registration for username {Username}", createDto.Username);
                return (ServiceResult.BadRequest, null, "Could not register user due to a save error.");
            }

            return (ServiceResult.Success, _mapper.Map<UserDto>(user), null);
        }

        public async Task<(ServiceResult Status, UserDto? Dto)> UpdateUserAsync(int userId, UserUpdateDto updateDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return (ServiceResult.NotFound, null);
            }

            // Check if email is being changed and if the new email already exists for another user
            if (user.Email != updateDto.Email && await _userRepository.GetByEmailAsync(updateDto.Email) != null)
            {
                return (ServiceResult.Conflict, null); // Email conflict
            }

            if (updateDto.UserGroupID.HasValue && await _userGroupRepository.GetByIdAsync(updateDto.UserGroupID.Value) == null)
            {
                return (ServiceResult.RefNotFound, null); // UserGroup not found
            }

            _mapper.Map(updateDto, user); // AutoMapper handles mapping updateDto to the existing user entity

            _userRepository.Update(user);
            if (!await _userRepository.SaveChangesAsync())
            {
                _logger.LogError("Failed to save updated user with ID {UserId}", userId);
                return (ServiceResult.BadRequest, null);
            }
            return (ServiceResult.Success, _mapper.Map<UserDto>(user));
        }


        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(PaginationParams paginationParams)
        {
            // In a real app, you'd use a paginated method in the repository
            var users = await _userRepository.GetAllAsync(); // This needs to be paginated
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<ServiceResult> AssignUserToGroupAsync(int userId, int? userGroupId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return ServiceResult.NotFound;

            if (userGroupId.HasValue)
            {
                var group = await _userGroupRepository.GetByIdAsync(userGroupId.Value);
                if (group == null) return ServiceResult.RefNotFound; // Group doesn't exist
            }

            user.UserGroupID = userGroupId;
            _userRepository.Update(user);
            return await _userRepository.SaveChangesAsync() ? ServiceResult.Success : ServiceResult.BadRequest;
        }

        public async Task<ServiceResult> ChangeUserRoleAsync(int userId, string newRole)
        {
            // Basic validation for role, more robust validation might be needed
            if (newRole != "Admin" && newRole != "User")
            {
                return ServiceResult.BadRequest;
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return ServiceResult.NotFound;

            user.Role = newRole;
            _userRepository.Update(user);
            return await _userRepository.SaveChangesAsync() ? ServiceResult.Success : ServiceResult.BadRequest;
        }

        public async Task<(ServiceResult Status, UserTokenDto? TokenDto, string? ErrorMessage)> AuthenticateUserAsync(UserLoginDto loginDto)
        {
            var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
            if (user == null || !user.IsActive || !_passwordHasher.VerifyPassword(user.PasswordHash, loginDto.Password))
            {
                return (ServiceResult.Unauthorized, null, "Invalid username or password.");
            }

            await UpdateLastLoginDateAsync(user.UserID);

            // Placeholder for JWT generation (Phase 7)
            // For now, we'll return user details that would go into a token.
            var userGroup = user.UserGroupID.HasValue ? await _userGroupRepository.GetByIdAsync(user.UserGroupID.Value) : null;
            var tokenDto = new UserTokenDto(
                Token: "DUMMY_JWT_TOKEN_REPLACE_LATER", // Placeholder
                UserID: user.UserID,
                Username: user.Username,
                Role: user.Role,
                UserGroupID: user.UserGroupID,
                GroupName: userGroup?.GroupName
            );

            return (ServiceResult.Success, tokenDto, null);
        }

        public async Task<ServiceResult> UpdateLastLoginDateAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return ServiceResult.NotFound;

            user.LastLoginDate = DateTime.UtcNow;
            _userRepository.Update(user);
            return await _userRepository.SaveChangesAsync() ? ServiceResult.Success : ServiceResult.BadRequest;
        }

        public async Task<ServiceResult> DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return ServiceResult.NotFound;

            // Business rule: Cannot delete currently logged-in user (example, needs context)
            // Business rule: Cannot delete the last admin (example)

            _userRepository.Delete(user);
            return await _userRepository.SaveChangesAsync() ? ServiceResult.Success : ServiceResult.BadRequest;
        }
    }
}

