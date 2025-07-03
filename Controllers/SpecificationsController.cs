using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using RegistryApi.DTOs;
using RegistryApi.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HelpersPaginationParams = RegistryApi.Helpers.PaginationParams;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // <-- Add this using statement

namespace RegistryApi.Controllers
{
    [Route("api/specifications")]
    [ApiController]
    [Authorize]
    public class SpecificationsController : ControllerBase
    {
        private readonly ISpecificationService _specificationService;
        private readonly ILogger<SpecificationsController> _logger;

        public SpecificationsController(ISpecificationService specificationService, ILogger<SpecificationsController> logger)
        {
            _specificationService = specificationService;
            _logger = logger;
        }

        // --- Helper method to get CurrentUserContext from Claims ---
        private CurrentUserContext? GetCurrentUserContextFromClaims()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return null; // Should not happen if [Authorize] is effective
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleClaim = User.FindFirstValue(ClaimTypes.Role);
            var userGroupIdClaim = User.FindFirstValue("UserGroupId"); // Custom claim

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId) || string.IsNullOrEmpty(roleClaim))
            {
                // Essential claims are missing or invalid
                _logger.LogWarning("Essential claims (UserID, Role) missing or invalid for authenticated user.");
                return null;
            }

            int? userGroupId = null;
            if (!string.IsNullOrEmpty(userGroupIdClaim) && int.TryParse(userGroupIdClaim, out var parsedUserGroupId))
            {
                userGroupId = parsedUserGroupId;
            }

            return new CurrentUserContext(userId, roleClaim, userGroupId);
        }

        // --- Specification Header Endpoints ---
        /// <summary>
        /// Gets a paginated list of publicly available specifications.
        /// This excludes specifications with a 'Submitted' or 'In progress' status that have not been checked by Admin.
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // This attribute makes this specific method publicly accessible
        [ProducesResponseType<PaginatedSpecificationHeaderResponse>(StatusCodes.Status200OK)]
        public async Task<Ok<PaginatedSpecificationHeaderResponse>> GetSpecifications([FromQuery] HelpersPaginationParams paginationParams)
        {
            var result = await _specificationService.GetSpecificationsAsync(paginationParams);
            return TypedResults.Ok(result);
        }
        /// <summary>
        /// Gets a paginated list of all available specifications.
        /// This includes all specifications irrespective of status.
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType<PaginatedSpecificationHeaderResponse>(StatusCodes.Status200OK)]
        public async Task<Ok<PaginatedSpecificationHeaderResponse>> GetAdminSpecifications([FromQuery] HelpersPaginationParams paginationParams)
        {
            var result = await _specificationService.GetAdminSpecificationsAsync(paginationParams);
            return TypedResults.Ok(result);
        }

        [HttpGet("by-user-group")]
        [ProducesResponseType<IEnumerable<SpecificationIdentifyingInformationHeaderDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<Results<Ok<IEnumerable<SpecificationIdentifyingInformationHeaderDto>>, ForbidHttpResult, BadRequest<string>, UnauthorizedHttpResult>> GetSpecificationsByUserGroup()
        {
            var currentUser = GetCurrentUserContextFromClaims();
            if (currentUser == null)
            {
                // ...
                return TypedResults.Unauthorized();
            }

            var (status, response) = await _specificationService.GetSpecificationsByUserGroupAsync(currentUser);

            return status switch
            {
                ServiceResult.Success => TypedResults.Ok(response!),
                ServiceResult.Forbidden => TypedResults.Forbid(),
                ServiceResult.Unauthorized => TypedResults.Unauthorized(),
                _ => TypedResults.BadRequest("Could not retrieve specifications for the user group.")
            };
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous] // This attribute makes this specific method publicly accessible
        [ProducesResponseType<SpecificationIdentifyingInformationDetailDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<Ok<SpecificationIdentifyingInformationDetailDto>, NotFound>> GetSpecification(int id)
        {
            var specification = await _specificationService.GetSpecificationByIdAsync(id);
            return specification == null ? TypedResults.NotFound() : TypedResults.Ok(specification);
        }

        [HttpPost]
        [ProducesResponseType<SpecificationIdentifyingInformationHeaderDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<Results<Created<SpecificationIdentifyingInformationHeaderDto>, BadRequest<string>, ForbidHttpResult, UnauthorizedHttpResult>> PostSpecification(
            [FromBody] SpecificationIdentifyingInformationCreateDto createDto)
        {
            if (!ModelState.IsValid) return TypedResults.BadRequest("Invalid specification data.");

            var currentUser = GetCurrentUserContextFromClaims();
            if (currentUser == null)
            {
                _logger.LogWarning("PostSpecification called by an authenticated user with missing/invalid claims.");
                return TypedResults.Unauthorized(); // Or BadRequest
            }

            var (status, createdSpecDto) = await _specificationService.CreateSpecificationAsync(createDto, currentUser);

            return status switch
            {
                ServiceResult.Success => TypedResults.Created($"/api/specifications/{createdSpecDto!.IdentityID}", createdSpecDto),
                ServiceResult.Forbidden => TypedResults.Forbid(),
                ServiceResult.Unauthorized => TypedResults.Unauthorized(), // If service layer explicitly deems it unauthorized
                _ => TypedResults.BadRequest("Could not create specification.")
            };
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<NoContent, BadRequest<string>, NotFound, ForbidHttpResult, UnauthorizedHttpResult>> PutSpecification(
            int id,
            [FromBody] SpecificationIdentifyingInformationUpdateDto updateDto)
        {
            if (!ModelState.IsValid) return TypedResults.BadRequest("Invalid specification update data.");

            var currentUser = GetCurrentUserContextFromClaims();
            if (currentUser == null)
            {
                _logger.LogWarning("PutSpecification called by an authenticated user with missing/invalid claims.");
                return TypedResults.Unauthorized(); // Or BadRequest
            }

            var result = await _specificationService.UpdateSpecificationAsync(id, updateDto, currentUser);

            return result switch
            {
                ServiceResult.Success => TypedResults.NoContent(),
                ServiceResult.NotFound => TypedResults.NotFound(),
                ServiceResult.Forbidden => TypedResults.Forbid(),
                ServiceResult.Unauthorized => TypedResults.Unauthorized(), // If service layer explicitly deems it unauthorized
                _ => TypedResults.BadRequest("Could not update specification.")
            };
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<Results<NoContent, NotFound, Conflict<string>, ForbidHttpResult, BadRequest<string>, UnauthorizedHttpResult>> DeleteSpecification(int id)
        {
            var currentUser = GetCurrentUserContextFromClaims();
            if (currentUser == null)
            {
                _logger.LogWarning("DeleteSpecification called by an authenticated user with missing/invalid claims.");
                return TypedResults.Unauthorized(); // Or BadRequest
            }

            var result = await _specificationService.DeleteSpecificationAsync(id, currentUser);

            return result switch
            {
                DeleteResult.Success => TypedResults.NoContent(),
                DeleteResult.NotFound => TypedResults.NotFound(),
                DeleteResult.Conflict => TypedResults.Conflict("Cannot delete specification because it has associated core or extension elements."),
                DeleteResult.Forbidden => TypedResults.Forbid(),
                DeleteResult.Error => TypedResults.BadRequest("Could not delete specification due to an error."),
                _ => TypedResults.BadRequest("Could not delete specification.")
            };
        }

        [HttpPut("{id:int}/assign-group/{groupId:int?}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<NoContent, BadRequest<string>, NotFound, ForbidHttpResult, UnauthorizedHttpResult>> AssignSpecificationToGroup(int id, int groupId)
        {
            var currentUser = GetCurrentUserContextFromClaims(); // Context still useful for logging, etc.
            if (currentUser == null) // Should be caught by [Authorize] but good for robustness
            {
                _logger.LogWarning("AssignSpecificationToGroup called by an unauthenticated user or token with missing claims.");
                return TypedResults.Unauthorized();
            }
            // Role check is handled by [Authorize(Roles = "Admin")], but an additional check here can be defensive
            if (currentUser.Role != "Admin")
            {
                return TypedResults.Forbid();
            }


            var result = await _specificationService.AssignSpecificationToGroupAsync(id, groupId, currentUser);

            return result switch
            {
                ServiceResult.Success => TypedResults.NoContent(),
                ServiceResult.NotFound => TypedResults.NotFound(),
                ServiceResult.Forbidden => TypedResults.Forbid(),
                ServiceResult.Unauthorized => TypedResults.Unauthorized(),
                _ => TypedResults.BadRequest("Could not assign specification to group.")
            };
        }



        // --- Specification Core Element Endpoints ---

        [HttpGet("{specificationId:int}/coreElements")]
        [AllowAnonymous]
        [ProducesResponseType<IEnumerable<SpecificationCoreDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<Ok<IEnumerable<SpecificationCoreDto>>, NotFound>> GetSpecificationCoreElements(
    int specificationId)
        {
            var result = await _specificationService.GetSpecificationCoresAsync(specificationId);
            return result == null ? TypedResults.NotFound() : TypedResults.Ok(result);
        }

        [HttpPost("{specificationId:int}/coreElements")]
        [ProducesResponseType<SpecificationCoreDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<Created<SpecificationCoreDto>, NotFound, BadRequest<string>, ForbidHttpResult, UnauthorizedHttpResult>> PostSpecificationCoreElement(
            int specificationId,
            [FromBody] SpecificationCoreCreateDto createDto)
        {
            if (!ModelState.IsValid) return TypedResults.BadRequest("Invalid core element data.");

            var currentUser = GetCurrentUserContextFromClaims();
            if (currentUser == null)
            {
                _logger.LogWarning("PostSpecificationCoreElement called by an authenticated user with missing/invalid claims.");
                return TypedResults.Unauthorized();
            }

            var (status, dto) = await _specificationService.AddCoreElementAsync(specificationId, createDto, currentUser);

            return status switch
            {
                ServiceResult.Success => TypedResults.Created($"/api/specifications/{specificationId}/coreElements/{dto!.EntityID}", dto),
                ServiceResult.NotFound => TypedResults.NotFound(),
                ServiceResult.RefNotFound => TypedResults.BadRequest("Referenced Core Invoice Model element not found."),
                ServiceResult.Forbidden => TypedResults.Forbid(),
                ServiceResult.Unauthorized => TypedResults.Unauthorized(),
                _ => TypedResults.BadRequest("Failed to add core element.")
            };
        }

        [HttpPut("{specificationId:int}/coreElements/{coreElementId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<NoContent, NotFound, BadRequest<string>, ForbidHttpResult, UnauthorizedHttpResult>> PutSpecificationCoreElement(
            int specificationId,
            int coreElementId,
            [FromBody] SpecificationCoreUpdateDto updateDto)
        {
            if (!ModelState.IsValid) return TypedResults.BadRequest("Invalid core element update data.");

            var currentUser = GetCurrentUserContextFromClaims();
            if (currentUser == null)
            {
                _logger.LogWarning("PutSpecificationCoreElement called by an authenticated user with missing/invalid claims.");
                return TypedResults.Unauthorized();
            }

            var result = await _specificationService.UpdateCoreElementAsync(specificationId, coreElementId, updateDto, currentUser);

            return result switch
            {
                ServiceResult.Success => TypedResults.NoContent(),
                ServiceResult.NotFound => TypedResults.NotFound(),
                ServiceResult.RefNotFound => TypedResults.BadRequest("Referenced Core Invoice Model element not found."),
                ServiceResult.Forbidden => TypedResults.Forbid(),
                ServiceResult.Unauthorized => TypedResults.Unauthorized(),
                _ => TypedResults.BadRequest("Failed to update core element.")
            };
        }

        [HttpDelete("{specificationId:int}/coreElements/{coreElementId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<NoContent, NotFound, ForbidHttpResult, BadRequest<string>, UnauthorizedHttpResult>> DeleteSpecificationCoreElement(
            int specificationId,
            int coreElementId)
        {
            var currentUser = GetCurrentUserContextFromClaims();
            if (currentUser == null)
            {
                _logger.LogWarning("DeleteSpecificationCoreElement called by an authenticated user with missing/invalid claims.");
                return TypedResults.Unauthorized();
            }

            var result = await _specificationService.DeleteCoreElementAsync(specificationId, coreElementId, currentUser);
            return result switch
            {
                ServiceResult.Success => TypedResults.NoContent(),
                ServiceResult.NotFound => TypedResults.NotFound(),
                ServiceResult.Forbidden => TypedResults.Forbid(),
                ServiceResult.Unauthorized => TypedResults.Unauthorized(),
                _ => TypedResults.BadRequest("Could not delete core element.")
            };
        }

        // --- Specification Extension Element Endpoints ---

        [HttpGet("{specificationId:int}/extensionElements")]
        [AllowAnonymous]
        [ProducesResponseType<IEnumerable<SpecificationExtensionComponentDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<Ok<IEnumerable<SpecificationExtensionComponentDto>>, NotFound>> GetSpecificationExtensionElements(
   int specificationId)
        {
            var result = await _specificationService.GetSpecificationExtensionsAsync(specificationId);
            return result == null ? TypedResults.NotFound() : TypedResults.Ok(result);
        }

        [HttpPost("{specificationId:int}/extensionElements")]
        [ProducesResponseType<SpecificationExtensionComponentDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<Created<SpecificationExtensionComponentDto>, NotFound, BadRequest<string>, ForbidHttpResult, UnauthorizedHttpResult>> PostSpecificationExtensionElement(
           int specificationId,
           [FromBody] SpecificationExtensionComponentCreateDto createDto)
        {
            if (!ModelState.IsValid) return TypedResults.BadRequest("Invalid extension element data.");

            var currentUser = GetCurrentUserContextFromClaims();
            if (currentUser == null)
            {
                _logger.LogWarning("PostSpecificationExtensionElement called by an authenticated user with missing/invalid claims.");
                return TypedResults.Unauthorized();
            }

            var (status, dto) = await _specificationService.AddExtensionElementAsync(specificationId, createDto, currentUser);

            return status switch
            {
                ServiceResult.Success => TypedResults.Created($"/api/specifications/{specificationId}/extensionElements/{dto!.EntityID}", dto),
                ServiceResult.NotFound => TypedResults.NotFound(),
                ServiceResult.RefNotFound => TypedResults.BadRequest("Referenced Extension Component element not found."),
                ServiceResult.Forbidden => TypedResults.Forbid(),
                ServiceResult.Unauthorized => TypedResults.Unauthorized(),
                _ => TypedResults.BadRequest("Failed to add extension element.")
            };
        }

        [HttpPut("{specificationId:int}/extensionElements/{extensionElementId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<NoContent, NotFound, BadRequest<string>, ForbidHttpResult, UnauthorizedHttpResult>> PutSpecificationExtensionElement(
           int specificationId,
           int extensionElementId,
           [FromBody] SpecificationExtensionComponentUpdateDto updateDto)
        {
            if (!ModelState.IsValid) return TypedResults.BadRequest("Invalid extension element update data.");

            var currentUser = GetCurrentUserContextFromClaims();
            if (currentUser == null)
            {
                _logger.LogWarning("PutSpecificationExtensionElement called by an authenticated user with missing/invalid claims.");
                return TypedResults.Unauthorized();
            }

            var result = await _specificationService.UpdateExtensionElementAsync(specificationId, extensionElementId, updateDto, currentUser);

            return result switch
            {
                ServiceResult.Success => TypedResults.NoContent(),
                ServiceResult.NotFound => TypedResults.NotFound(),
                ServiceResult.RefNotFound => TypedResults.BadRequest("Referenced Extension Component element not found."),
                ServiceResult.Forbidden => TypedResults.Forbid(),
                ServiceResult.Unauthorized => TypedResults.Unauthorized(),
                _ => TypedResults.BadRequest("Failed to update extension element.")
            };
        }

        [HttpDelete("{specificationId:int}/extensionElements/{extensionElementId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<NoContent, NotFound, ForbidHttpResult, BadRequest<string>, UnauthorizedHttpResult>> DeleteSpecificationExtensionElement(
           int specificationId,
           int extensionElementId)
        {
            var currentUser = GetCurrentUserContextFromClaims();
            if (currentUser == null)
            {
                _logger.LogWarning("DeleteSpecificationExtensionElement called by an authenticated user with missing/invalid claims.");
                return TypedResults.Unauthorized();
            }

            var result = await _specificationService.DeleteExtensionElementAsync(specificationId, extensionElementId, currentUser);
            return result switch
            {
                ServiceResult.Success => TypedResults.NoContent(),
                ServiceResult.NotFound => TypedResults.NotFound(),
                ServiceResult.Forbidden => TypedResults.Forbid(),
                ServiceResult.Unauthorized => TypedResults.Unauthorized(),
                _ => TypedResults.BadRequest("Could not delete extension element.")
            };
        }
    }
}