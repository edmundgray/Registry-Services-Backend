using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults; // For TypedResults
using RegistryApi.DTOs;
using RegistryApi.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using RegistryApi.Helpers; // For IEnumerable

namespace RegistryApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        // Simulated Admin context for Admin-only actions (TEMPORARY - REPLACE IN PHASE 7)
        private CurrentUserContext GetAdminContext() => new CurrentUserContext(0, "Admin", null);


        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
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

        // GET: api/users (Admin Only)
        [HttpGet]
        [ProducesResponseType<IEnumerable<UserDto>>(StatusCodes.Status200OK)]
        // Add [Authorize(Roles = "Admin")] in Phase 7
        public async Task<Ok<IEnumerable<UserDto>>> GetAllUsers([FromQuery] PaginationParams paginationParams)
        {
            // In a real scenario, check if current user is Admin before proceeding
            // For now, directly calling service.
            var users = await _userService.GetAllUsersAsync(paginationParams);
            return TypedResults.Ok(users);
        }

        // GET: api/users/{id} (Admin Only)
        [HttpGet("{id:int}")]
        [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // Add [Authorize(Roles = "Admin")] in Phase 7
        public async Task<Results<Ok<UserDto>, NotFound>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return user == null ? TypedResults.NotFound() : TypedResults.Ok(user);
        }

        // PUT: api/users/{id} (Admin Only)
        [HttpPut("{id:int}")]
        [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        // Add [Authorize(Roles = "Admin")] in Phase 7
        public async Task<Results<Ok<UserDto>, BadRequest<string>, NotFound, Conflict<string>>> UpdateUser(int id, [FromBody] UserUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return TypedResults.BadRequest("Invalid user update data.");
            }
            var (status, userDto) = await _userService.UpdateUserAsync(id, updateDto);
            return status switch
            {
                ServiceResult.Success => TypedResults.Ok(userDto!),
                ServiceResult.NotFound => TypedResults.NotFound(),
                ServiceResult.Conflict => TypedResults.Conflict("Update conflict, e.g., email already in use."),
                _ => TypedResults.BadRequest("Could not update user.")
            };
        }

        // DELETE: api/users/{id} (Admin Only)
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // Add [Authorize(Roles = "Admin")] in Phase 7
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

        // PUT: api/users/{id}/assign-group (Admin Only)
        [HttpPut("{id:int}/assign-group")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // Add [Authorize(Roles = "Admin")] in Phase 7
        public async Task<Results<NoContent, BadRequest<string>, NotFound>> AssignUserToGroup(int id, [FromBody] int? userGroupId) // DTO might be better for body
        {
            var result = await _userService.AssignUserToGroupAsync(id, userGroupId);
            return result switch
            {
                ServiceResult.Success => TypedResults.NoContent(),
                ServiceResult.NotFound => TypedResults.NotFound(), // User or Group not found
                ServiceResult.RefNotFound => TypedResults.BadRequest("UserGroup not found."),
                _ => TypedResults.BadRequest("Could not assign user to group.")
            };
        }

        // PUT: api/users/{id}/change-role (Admin Only)
        [HttpPut("{id:int}/change-role")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // Add [Authorize(Roles = "Admin")] in Phase 7
        public async Task<Results<NoContent, BadRequest<string>, NotFound>> ChangeUserRole(int id, [FromBody] string newRole) // DTO might be better for body
        {
            if (string.IsNullOrWhiteSpace(newRole)) return TypedResults.BadRequest("New role cannot be empty.");
            var result = await _userService.ChangeUserRoleAsync(id, newRole);
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

