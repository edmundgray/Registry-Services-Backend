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
            var resultDto = new UserGroupDto(group.UserGroupID, group.GroupName, group.Description, group.CreatedDate, 0, 0, 0, 0, 0, 0);
            return (ServiceResult.Success, resultDto);
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
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to delete user group with ID {GroupId} due to a database update exception. It might be in use.", groupId);
                return ServiceResult.Conflict;
            }
        }

        public async Task<UserGroupDto?> GetUserGroupByIdAsync(int groupId)
        {
            var groups = await _userGroupRepository.GetAllWithCountsAsync();
            return groups.FirstOrDefault(g => g.UserGroupID == groupId);
        }

        public async Task<IEnumerable<UserGroupDto>> GetAllUserGroupsAsync()
        {
            return await _userGroupRepository.GetAllWithCountsAsync();
        }
    }
}
