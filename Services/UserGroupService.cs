using AutoMapper;
using RegistryApi.DTOs;
using RegistryApi.Models;
using RegistryApi.Repositories;
using RegistryApi.Helpers; // For PagedList
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace RegistryApi.Services
{
    public class UserGroupService : IUserGroupService
    {
        private readonly IUserGroupRepository _userGroupRepository;
        private readonly IUserRepository _userRepository; // To check if users are in group before deletion
        private readonly IMapper _mapper;
        private readonly ILogger<UserGroupService> _logger;


        public UserGroupService(
            IUserGroupRepository userGroupRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<UserGroupService> logger)
        {
            _userGroupRepository = userGroupRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<(ServiceResult Status, UserGroupDto? Dto)> CreateUserGroupAsync(UserGroupCreateDto createDto)
        {
            if (await _userGroupRepository.GetByNameAsync(createDto.GroupName) != null)
            {
                return (ServiceResult.Conflict, null); // Group name already exists
            }

            var group = _mapper.Map<UserGroup>(createDto);
            // CreatedDate is set by AutoMapper profile

            await _userGroupRepository.AddAsync(group);
            if (!await _userGroupRepository.SaveChangesAsync())
            {
                _logger.LogError("Failed to save new user group with name {GroupName}", createDto.GroupName);
                return (ServiceResult.BadRequest, null);
            }
            return (ServiceResult.Success, _mapper.Map<UserGroupDto>(group));
        }

        public async Task<ServiceResult> UpdateUserGroupAsync(int groupId, UserGroupUpdateDto updateDto)
        {
            var group = await _userGroupRepository.GetByIdAsync(groupId);
            if (group == null)
            {
                return ServiceResult.NotFound;
            }

            // Check if group name is being changed and if the new name already exists
            if (group.GroupName != updateDto.GroupName && await _userGroupRepository.GetByNameAsync(updateDto.GroupName) != null)
            {
                return ServiceResult.Conflict; // New group name already exists
            }

            _mapper.Map(updateDto, group);
            _userGroupRepository.Update(group);

            if (!await _userGroupRepository.SaveChangesAsync())
            {
                _logger.LogError("Failed to save updated user group with ID {GroupId}", groupId);
                return ServiceResult.BadRequest;
            }
            return ServiceResult.Success;
        }

        public async Task<ServiceResult> DeleteUserGroupAsync(int groupId)
        {
            var group = await _userGroupRepository.GetByIdAsync(groupId);
            if (group == null)
            {
                return ServiceResult.NotFound;
            }

            // Per plan: Prevent deletion if users are assigned (due to OnDelete.Restrict on User.UserGroupID FK)
            // EF Core will throw an exception if users are in this group due to the FK constraint.
            // We can check explicitly for a better error message if desired, but the DB will enforce it.
            // Example explicit check:
            // var usersInGroup = await _userRepository.GetAllAsync(); // This is inefficient, need a specific repo method
            // if (usersInGroup.Any(u => u.UserGroupID == groupId))
            // {
            //     return ServiceResult.Conflict; // Users are still assigned to this group
            // }


            // Note: SpecificationIdentifyingInformation.UserGroupID is SetNull on delete, so that's handled by DB.
            try
            {
                _userGroupRepository.Delete(group);
                if (!await _userGroupRepository.SaveChangesAsync())
                {
                    _logger.LogError("Failed to delete user group with ID {GroupId} during save changes.", groupId);
                    return ServiceResult.BadRequest;
                }
                return ServiceResult.Success;
            }
            catch (DbUpdateException ex) // Catches errors from DB, like FK violations
            {
                _logger.LogError(ex, "Failed to delete user group with ID {GroupId} due to a database update exception. It might be in use.", groupId);
                // This will likely be triggered by the Restrict constraint if users are in the group.
                return ServiceResult.Conflict; // Or BadRequest, depending on how you want to signal this
            }
        }

        public async Task<UserGroupDto?> GetUserGroupByIdAsync(int groupId)
        {
            var group = await _userGroupRepository.GetByIdAsync(groupId);
            return _mapper.Map<UserGroupDto>(group);
        }

        public async Task<IEnumerable<UserGroupDto>> GetAllUserGroupsAsync(PaginationParams paginationParams)
        {
            // In a real app, you'd use a paginated method in the repository
            var groups = await _userGroupRepository.GetAllAsync(); // This needs to be paginated
            return _mapper.Map<IEnumerable<UserGroupDto>>(groups);
        }
    }
}

