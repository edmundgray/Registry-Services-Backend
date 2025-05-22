using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults; // For TypedResults
using RegistryApi.DTOs;
using RegistryApi.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using RegistryApi.Helpers;// For StatusCodes
using Microsoft.AspNetCore.Authorization; 

namespace RegistryApi.Controllers
{
    [Route("api/usergroups")]
    [ApiController]
    [Authorize(Roles = "Admin")] 
    public class UserGroupsController : ControllerBase
    {
        private readonly IUserGroupService _userGroupService;
        private readonly ILogger<UserGroupsController> _logger;

        public UserGroupsController(IUserGroupService userGroupService, ILogger<UserGroupsController> logger)
        {
            _userGroupService = userGroupService;
            _logger = logger;
        }

        // POST: api/usergroups (Admin Only)
        [HttpPost]
        [ProducesResponseType<UserGroupDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<Results<Created<UserGroupDto>, BadRequest<string>, Conflict<string>>> CreateUserGroup([FromBody] UserGroupCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return TypedResults.BadRequest("Invalid user group data.");
            }
            var (status, groupDto) = await _userGroupService.CreateUserGroupAsync(createDto);
            return status switch
            {
                ServiceResult.Success => TypedResults.Created($"/api/usergroups/{groupDto!.UserGroupID}", groupDto),
                ServiceResult.Conflict => TypedResults.Conflict("User group name already exists."),
                _ => TypedResults.BadRequest("Could not create user group.")
            };
        }

        // GET: api/usergroups (Admin Only)
        [HttpGet]
        [ProducesResponseType<IEnumerable<UserGroupDto>>(StatusCodes.Status200OK)]
        public async Task<Ok<IEnumerable<UserGroupDto>>> GetAllUserGroups([FromQuery] PaginationParams paginationParams)
        {
            var groups = await _userGroupService.GetAllUserGroupsAsync(paginationParams);
            return TypedResults.Ok(groups);
        }

        // GET: api/usergroups/{id} (Admin Only)
        [HttpGet("{id:int}")]
        [ProducesResponseType<UserGroupDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<Ok<UserGroupDto>, NotFound>> GetUserGroupById(int id)
        {
            var group = await _userGroupService.GetUserGroupByIdAsync(id);
            return group == null ? TypedResults.NotFound() : TypedResults.Ok(group);
        }

        // PUT: api/usergroups/{id} (Admin Only)
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<Results<NoContent, BadRequest<string>, NotFound, Conflict<string>>> UpdateUserGroup(int id, [FromBody] UserGroupUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return TypedResults.BadRequest("Invalid user group update data.");
            }
            var result = await _userGroupService.UpdateUserGroupAsync(id, updateDto);
            return result switch
            {
                ServiceResult.Success => TypedResults.NoContent(),
                ServiceResult.NotFound => TypedResults.NotFound(),
                ServiceResult.Conflict => TypedResults.Conflict("Update conflict, e.g., group name already in use."),
                _ => TypedResults.BadRequest("Could not update user group.")
            };
        }

        // DELETE: api/usergroups/{id} (Admin Only)
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)] // If group is in use by users
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Added for the default case
        public async Task<Results<NoContent, NotFound, Conflict<string>, BadRequest<string>>> DeleteUserGroup(int id)
        {
            var result = await _userGroupService.DeleteUserGroupAsync(id);
            return result switch
            {
                ServiceResult.Success => TypedResults.NoContent(),
                ServiceResult.NotFound => TypedResults.NotFound(),
                ServiceResult.Conflict => TypedResults.Conflict("Cannot delete group. It may be assigned to users."),
                _ => TypedResults.BadRequest("Could not delete user group.")
            };
        }
    }
}
