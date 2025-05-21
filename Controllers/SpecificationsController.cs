using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // For StatusCodes
using Microsoft.AspNetCore.Http.HttpResults; // Required for TypedResults and ForbidHttpResult
using RegistryApi.DTOs; // Kept for other DTOs like PaginatedSpecificationHeaderResponse, etc.
using RegistryApi.Services;
using System.Threading.Tasks; // Required for Task
using Microsoft.Extensions.Logging; // Required for ILogger
using HelpersPaginationParams = RegistryApi.Helpers.PaginationParams; // Alias for PaginationParams from Helpers namespace

namespace RegistryApi.Controllers;

[Route("api/specifications")]
[ApiController]
public class SpecificationsController : ControllerBase
{
    private readonly ISpecificationService _specificationService;
    private readonly ILogger<SpecificationsController> _logger;

    // --- TEMPORARY: Simulated User Context for testing ---
    // In Phase 7 (Authentication), this will be replaced by getting user from HttpContext.User
    private CurrentUserContext GetSimulatedAdminContext() => new CurrentUserContext(UserId: 1, Role: "Admin", UserGroupId: null);
    // Example simulated regular user (adjust ID and GroupID as needed for testing)
    private CurrentUserContext GetSimulatedUserContext() => new CurrentUserContext(UserId: 2, Role: "User", UserGroupId: 1);
    // ----------------------------------------------------

    public SpecificationsController(ISpecificationService specificationService, ILogger<SpecificationsController> logger)
    {
        _specificationService = specificationService;
        _logger = logger;
    }

    // --- Specification Header Endpoints ---

    [HttpGet]
    [ProducesResponseType<PaginatedSpecificationHeaderResponse>(StatusCodes.Status200OK)]
    public async Task<Ok<PaginatedSpecificationHeaderResponse>> GetSpecifications([FromQuery] HelpersPaginationParams paginationParams)
    {
        var result = await _specificationService.GetSpecificationsAsync(paginationParams);
        return TypedResults.Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<SpecificationIdentifyingInformationDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<Ok<SpecificationIdentifyingInformationDetailDto>, NotFound>> GetSpecification(
        int id,
        [FromQuery] HelpersPaginationParams coreParams,
        [FromQuery] HelpersPaginationParams extParams)
    {
        var specification = await _specificationService.GetSpecificationByIdAsync(id, coreParams, extParams);
        return specification == null ? TypedResults.NotFound() : TypedResults.Ok(specification);
    }

    [HttpPost]
    [ProducesResponseType<SpecificationIdentifyingInformationHeaderDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<Results<Created<SpecificationIdentifyingInformationHeaderDto>, BadRequest<string>, UnauthorizedHttpResult, ForbidHttpResult>> PostSpecification(
        [FromBody] SpecificationIdentifyingInformationCreateDto createDto)
    {
        if (!ModelState.IsValid) return TypedResults.BadRequest("Invalid specification data.");

        // TEMPORARY: Simulate a logged-in user. Replace with actual user context in Phase 7.
        // For testing, let's assume a regular user is creating this.
        var currentUser = GetSimulatedUserContext();
        // If testing Admin creation, use: var currentUser = GetSimulatedAdminContext();

        var (status, createdSpecDto) = await _specificationService.CreateSpecificationAsync(createDto, currentUser);

        return status switch
        {
            ServiceResult.Success => TypedResults.Created($"/api/specifications/{createdSpecDto!.IdentityID}", createdSpecDto),
            ServiceResult.Unauthorized => TypedResults.Unauthorized(),
            ServiceResult.Forbidden => TypedResults.Forbid(),
            _ => TypedResults.BadRequest("Could not create specification.")
        };
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<NoContent, BadRequest<string>, NotFound, UnauthorizedHttpResult, ForbidHttpResult>> PutSpecification(
        int id,
        [FromBody] SpecificationIdentifyingInformationUpdateDto updateDto)
    {
        if (!ModelState.IsValid) return TypedResults.BadRequest("Invalid specification update data.");

        // TEMPORARY: Simulate. Admins can edit, users can edit if their group owns it.
        var currentUser = GetSimulatedUserContext(); // Or GetSimulatedAdminContext() for testing admin path

        var result = await _specificationService.UpdateSpecificationAsync(id, updateDto, currentUser);

        return result switch
        {
            ServiceResult.Success => TypedResults.NoContent(),
            ServiceResult.NotFound => TypedResults.NotFound(),
            ServiceResult.Forbidden => TypedResults.Forbid(),
            ServiceResult.Unauthorized => TypedResults.Unauthorized(),
            _ => TypedResults.BadRequest("Could not update specification.")
        };
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<Results<NoContent, NotFound, Conflict<string>, UnauthorizedHttpResult, ForbidHttpResult, BadRequest<string>>> DeleteSpecification(int id)
    {
        // TEMPORARY: Simulate.
        var currentUser = GetSimulatedAdminContext(); // Deletion might often be an Admin task, or a user deleting their own.

        var result = await _specificationService.DeleteSpecificationAsync(id, currentUser);

        return result switch
        {
            DeleteResult.Success => TypedResults.NoContent(),
            DeleteResult.NotFound => TypedResults.NotFound(),
            DeleteResult.Conflict => TypedResults.Conflict("Cannot delete specification because it has associated core or extension elements."),
            DeleteResult.Forbidden => TypedResults.Forbid(),
            DeleteResult.Error => TypedResults.BadRequest("Could not delete specification due to an error."), // Map Error to BadRequest
            _ => TypedResults.BadRequest("Could not delete specification.")
        };
    }

    // PUT: api/specifications/{id}/assign-group/{groupId} (Admin Only)
    [HttpPut("{id:int}/assign-group/{groupId:int?}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<NoContent, BadRequest<string>, NotFound, UnauthorizedHttpResult, ForbidHttpResult>> AssignSpecificationToGroup(int id, int? groupId)
    {
        var currentUser = GetSimulatedAdminContext(); // This is an Admin action
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

    // PUT: api/specifications/{id}/remove-group (Admin Only)
    [HttpPut("{id:int}/remove-group")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<NoContent, NotFound, UnauthorizedHttpResult, ForbidHttpResult, BadRequest<string>>> RemoveSpecificationFromGroup(int id)
    {
        var currentUser = GetSimulatedAdminContext(); // This is an Admin action
        var result = await _specificationService.AssignSpecificationToGroupAsync(id, null, currentUser); // Assign to null group

        return result switch
        {
            ServiceResult.Success => TypedResults.NoContent(),
            ServiceResult.NotFound => TypedResults.NotFound(),
            ServiceResult.Forbidden => TypedResults.Forbid(),
            ServiceResult.Unauthorized => TypedResults.Unauthorized(),
            _ => TypedResults.BadRequest("Could not remove specification from group.")
        };
    }


    // --- Specification Core Element Endpoints ---

    [HttpGet("{specificationId:int}/coreElements")]
    [ProducesResponseType<PaginatedSpecificationCoreResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<Ok<PaginatedSpecificationCoreResponse>, NotFound>> GetSpecificationCoreElements(
        int specificationId,
        [FromQuery] HelpersPaginationParams paginationParams)
    {
        var result = await _specificationService.GetSpecificationCoresAsync(specificationId, paginationParams);
        return result == null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    [HttpPost("{specificationId:int}/coreElements")]
    [ProducesResponseType<SpecificationCoreDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<Created<SpecificationCoreDto>, NotFound, BadRequest<string>, UnauthorizedHttpResult, ForbidHttpResult>> PostSpecificationCoreElement(
        int specificationId,
        [FromBody] SpecificationCoreCreateDto createDto)
    {
        if (!ModelState.IsValid) return TypedResults.BadRequest("Invalid core element data.");
        var currentUser = GetSimulatedUserContext();
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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<NoContent, NotFound, BadRequest<string>, UnauthorizedHttpResult, ForbidHttpResult>> PutSpecificationCoreElement(
        int specificationId,
        int coreElementId,
        [FromBody] SpecificationCoreUpdateDto updateDto)
    {
        if (!ModelState.IsValid) return TypedResults.BadRequest("Invalid core element update data.");
        var currentUser = GetSimulatedUserContext();
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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<NoContent, NotFound, UnauthorizedHttpResult, ForbidHttpResult, BadRequest<string>>> DeleteSpecificationCoreElement(
        int specificationId,
        int coreElementId)
    {
        var currentUser = GetSimulatedUserContext();
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
    [ProducesResponseType<PaginatedSpecificationExtensionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<Ok<PaginatedSpecificationExtensionResponse>, NotFound>> GetSpecificationExtensionElements(
       int specificationId,
       [FromQuery] HelpersPaginationParams paginationParams)
    {
        var result = await _specificationService.GetSpecificationExtensionsAsync(specificationId, paginationParams);
        return result == null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    [HttpPost("{specificationId:int}/extensionElements")]
    [ProducesResponseType<SpecificationExtensionComponentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<Created<SpecificationExtensionComponentDto>, NotFound, BadRequest<string>, UnauthorizedHttpResult, ForbidHttpResult>> PostSpecificationExtensionElement(
       int specificationId,
       [FromBody] SpecificationExtensionComponentCreateDto createDto)
    {
        if (!ModelState.IsValid) return TypedResults.BadRequest("Invalid extension element data.");
        var currentUser = GetSimulatedUserContext();
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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<NoContent, NotFound, BadRequest<string>, UnauthorizedHttpResult, ForbidHttpResult>> PutSpecificationExtensionElement(
       int specificationId,
       int extensionElementId,
       [FromBody] SpecificationExtensionComponentUpdateDto updateDto)
    {
        if (!ModelState.IsValid) return TypedResults.BadRequest("Invalid extension element update data.");
        var currentUser = GetSimulatedUserContext();
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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<NoContent, NotFound, UnauthorizedHttpResult, ForbidHttpResult, BadRequest<string>>> DeleteSpecificationExtensionElement(
       int specificationId,
       int extensionElementId)
    {
        var currentUser = GetSimulatedUserContext();
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
