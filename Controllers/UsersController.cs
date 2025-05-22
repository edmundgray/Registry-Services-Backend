using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using RegistryApi.DTOs;
using RegistryApi.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using RegistryApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // <-- Add this using statement

namespace RegistryApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        // Simulated Admin context is removed as it's no longer needed
        // after applying [Authorize(Roles="Admin")] and real context derivation.

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // Helper method (can be extracted to a base controller or helper class if used in many controllers)
        private CurrentUserContext? GetCurrentUserContextFromClaims()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirstValue(ClaimTypes.Role);
            var userGroupIdClaim = User.FindFirstValue("UserGroupId");

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId) || string.IsNullOrEmpty(roleClaim))
            {
                _logger.LogWarning("Essential claims (UserID, Role) missing or invalid for authenticated user in UsersController.");
                return null;
            }

            int? userGroupId = null;
            if (!string.IsNullOrEmpty(userGroupIdClaim) && int.TryParse(userGroupIdClaim, out var parsedUserGroupId))
            {
                userGroupId = parsedUserGroupId;
            }
            return new CurrentUserContext(userId, roleClaim, userGroupId);
        }


        // POST: api/users/register
        [HttpPost("register")]
        [ProducesResponseType<UserDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<Results<Created<UserDto>, BadRequest<string>, Conflict<string>>> RegisterUser([FromBody] UserCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return TypedResults.BadRequest("Invalid user registration data.");
            }

            var (status, userDto, errorMessage) = await _userService.RegisterUserAsync(createDto);

            return status switch
            {
                ServiceResult.Success => TypedResults.Created($"/api/users/{userDto!.UserID}", userDto),
                ServiceResult.Conflict => TypedResults.Conflict(errorMessage ?? "User registration conflict."),
                _ => TypedResults.BadRequest(errorMessage ?? "Could not register user.")
            };
        }

        // GET: api/users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<IEnumerable<UserDto>>(StatusCodes.Status200OK)]
        public async Task<Ok<IEnumerable<UserDto>>> GetAllUsers([FromQuery] PaginationParams paginationParams)
        {
            // CurrentUserContext not strictly needed for this service call if it's purely Admin-gated
            // but could be passed for logging or more complex scenarios.
            var users = await _userService.GetAllUsersAsync(paginationParams);
            return TypedResults.Ok(users);
        }

        // GET: api/users/{id}
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<Ok<UserDto>, NotFound>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return user == null ? TypedResults.NotFound() : TypedResults.Ok(user);
        }

        // PUT: api/users/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<Results<Ok<UserDto>, BadRequest<string>, NotFound, Conflict<string>>> UpdateUser(int id, [FromBody] UserUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return TypedResults.BadRequest("Invalid user update data.");
            }
            // CurrentUserContext could be passed to _userService.UpdateUserAsync if the service
            // needed to know which admin performed the action, but the method signature doesn't currently require it.
            var (status, userDto) = await _userService.UpdateUserAsync(id, updateDto);
            return status switch
            {
                ServiceResult.Success => TypedResults.Ok(userDto!),
                ServiceResult.NotFound => TypedResults.NotFound(),
                ServiceResult.Conflict => TypedResults.Conflict("Update conflict, e.g., email already in use."),
                _ => TypedResults.BadRequest("Could not update user.")
            };
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<NoContent, NotFound, BadRequest<string>>> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            return result switch
            {
                ServiceResult.Success => TypedResults.NoContent(),
                ServiceResult.NotFound => TypedResults.NotFound(),
                _ => TypedResults.BadRequest("Could not delete user.")
            };
        }

        // PUT: api/users/{id}/assign-group
        [HttpPut("{id:int}/assign-group")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<NoContent, BadRequest<string>, NotFound>> AssignUserToGroup(int id, [FromBody] UserGroupAssignmentDto dto)
        {
            var result = await _userService.AssignUserToGroupAsync(id, dto.UserGroupId);
            return result switch
            {
                ServiceResult.Success => TypedResults.NoContent(),
                ServiceResult.NotFound => TypedResults.NotFound(),
                ServiceResult.RefNotFound => TypedResults.BadRequest("UserGroup not found."),
                _ => TypedResults.BadRequest("Could not assign user to group.")
            };
        }
        public record UserGroupAssignmentDto(int? UserGroupId); // DTO for request body


        // PUT: api/users/{id}/change-role
        [HttpPut("{id:int}/change-role")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<NoContent, BadRequest<string>, NotFound>> ChangeUserRole(int id, [FromBody] RoleChangeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewRole)) return TypedResults.BadRequest("New role cannot be empty.");
            var result = await _userService.ChangeUserRoleAsync(id, dto.NewRole);
            return result switch
            {
                ServiceResult.Success => TypedResults.NoContent(),
                ServiceResult.NotFound => TypedResults.NotFound(),
                ServiceResult.BadRequest => TypedResults.BadRequest("Invalid role specified or could not change role."),
                _ => TypedResults.BadRequest("Could not change user role.")
            };
        }
    }

   
}