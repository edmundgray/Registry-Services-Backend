using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults; // Required for TypedResults
using RegistryApi.DTOs;
using RegistryApi.Services;
using RegistryApi.Helpers;


namespace RegistryApi.Controllers;

[Route("api/specifications")]
[ApiController]
// Using primary constructor for dependency injection
public class SpecificationsController(ISpecificationService specificationService, ILogger<SpecificationsController> logger) : ControllerBase
{
    // --- Specification Header Endpoints ---

    // GET: api/specifications
    [HttpGet]
    [ProducesResponseType<PaginatedSpecificationHeaderResponse>(StatusCodes.Status200OK)]
    public async Task<Ok<PaginatedSpecificationHeaderResponse>> GetSpecifications([FromQuery] PaginationParams paginationParams)
    {
        var result = await specificationService.GetSpecificationsAsync(paginationParams);
        // Using TypedResults.Ok for strong typing and clarity
        return TypedResults.Ok(result);
    }

    // GET: api/specifications/5
    [HttpGet("{id:int}")] // Route constraint for id
    [ProducesResponseType<SpecificationIdentifyingInformationDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<Ok<SpecificationIdentifyingInformationDetailDto>, NotFound>> GetSpecification(
        int id,
        [FromQuery] PaginationParams coreParams,
        [FromQuery] PaginationParams extParams)
    {
        var specification = await specificationService.GetSpecificationByIdAsync(id, coreParams, extParams);

        return specification == null
            ? TypedResults.NotFound()
            : TypedResults.Ok(specification);
    }

    // POST: api/specifications
    [HttpPost]
    [ProducesResponseType<SpecificationIdentifyingInformationHeaderDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<Results<Created<SpecificationIdentifyingInformationHeaderDto>, BadRequest<string>>> PostSpecification(
        [FromBody] SpecificationIdentifyingInformationCreateDto createDto)
    {
        // ModelState validation happens automatically with [ApiController]

        var createdSpecification = await specificationService.CreateSpecificationAsync(createDto);

        if (createdSpecification == null)
        {
             return TypedResults.BadRequest("Could not create specification.");
        }

        // Return 201 Created with the object, location header is harder with TypedResults directly here
        // Consider returning the object directly or using ActionResult for Location header if needed
        return TypedResults.Created($"/api/specifications/{createdSpecification.IdentityID}", createdSpecification);
    }

    // PUT: api/specifications/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Handled by framework model binding/validation
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<NoContent, NotFound>> PutSpecification(
        int id,
        [FromBody] SpecificationIdentifyingInformationUpdateDto updateDto)
    {
        var success = await specificationService.UpdateSpecificationAsync(id, updateDto);

        return success
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    // DELETE: api/specifications/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<Results<NoContent, NotFound, Conflict<string>>> DeleteSpecification(int id)
    {
        var result = await specificationService.DeleteSpecificationAsync(id);

        return result switch
        {
            DeleteResult.Success => TypedResults.NoContent(),
            DeleteResult.NotFound => TypedResults.NotFound(),
            DeleteResult.Conflict => TypedResults.Conflict("Cannot delete specification because it has associated core or extension elements."),
            _ => throw new InvalidOperationException("Unexpected delete result") // Should not happen
        };
    }


    // --- Specification Core Element Endpoints ---

    // GET: api/specifications/5/coreElements
    [HttpGet("{specificationId:int}/coreElements")]
    [ProducesResponseType<PaginatedSpecificationCoreResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<Ok<PaginatedSpecificationCoreResponse>, NotFound>> GetSpecificationCoreElements(
        int specificationId,
        [FromQuery] PaginationParams paginationParams)
    {
        var result = await specificationService.GetSpecificationCoresAsync(specificationId, paginationParams);
        return result == null
            ? TypedResults.NotFound() // Specification itself not found
            : TypedResults.Ok(result);
    }

    // POST: api/specifications/5/coreElements
    [HttpPost("{specificationId:int}/coreElements")]
    [ProducesResponseType<RegistryApi.DTOs. SpecificationCoreDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<Created<SpecificationCoreDto>, NotFound, BadRequest<string>>> PostSpecificationCoreElement(
        int specificationId,
        [FromBody] SpecificationCoreCreateDto createDto)
    {
         var (status, dto) = await specificationService.AddCoreElementAsync(specificationId, createDto);

         return status switch
         {
             ServiceResult.Success => TypedResults.Created($"/api/specifications/{specificationId}/coreElements/{dto!.EntityID}", dto), // Assuming GetById exists
             ServiceResult.NotFound => TypedResults.NotFound(), // Parent spec not found
             ServiceResult.RefNotFound => TypedResults.BadRequest("Referenced Core Invoice Model element not found."),
             _ => TypedResults.BadRequest("Failed to add core element.")
         };
    }


    // PUT: api/specifications/5/coreElements/10
    [HttpPut("{specificationId:int}/coreElements/{coreElementId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<NoContent, NotFound, BadRequest<string>>> PutSpecificationCoreElement(
        int specificationId,
        int coreElementId,
        [FromBody] SpecificationCoreUpdateDto updateDto)
    {
         var result = await specificationService.UpdateCoreElementAsync(specificationId, coreElementId, updateDto);

         return result switch
         {
             ServiceResult.Success => TypedResults.NoContent(),
             ServiceResult.NotFound => TypedResults.NotFound(), // Core element within spec not found
             ServiceResult.RefNotFound => TypedResults.BadRequest("Referenced Core Invoice Model element not found."),
             _ => TypedResults.BadRequest("Failed to update core element.")
         };
    }

    // DELETE: api/specifications/5/coreElements/10
    [HttpDelete("{specificationId:int}/coreElements/{coreElementId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<NoContent, NotFound>> DeleteSpecificationCoreElement(
        int specificationId,
        int coreElementId)
    {
         var result = await specificationService.DeleteCoreElementAsync(specificationId, coreElementId);
         return result == ServiceResult.Success
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }


    // --- Specification Extension Element Endpoints ---

    // GET: api/specifications/5/extensionElements
    [HttpGet("{specificationId:int}/extensionElements")]
    [ProducesResponseType<PaginatedSpecificationExtensionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
     public async Task<Results<Ok<PaginatedSpecificationExtensionResponse>, NotFound>> GetSpecificationExtensionElements(
        int specificationId,
        [FromQuery] PaginationParams paginationParams)
    {
        var result = await specificationService.GetSpecificationExtensionsAsync(specificationId, paginationParams);
        return result == null
            ? TypedResults.NotFound() // Specification itself not found
            : TypedResults.Ok(result);
    }

    // POST: api/specifications/5/extensionElements
    [HttpPost("{specificationId:int}/extensionElements")]
    [ProducesResponseType<SpecificationExtensionComponentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
     public async Task<Results<Created<SpecificationExtensionComponentDto>, NotFound, BadRequest<string>>> PostSpecificationExtensionElement(
        int specificationId,
        [FromBody] SpecificationExtensionComponentCreateDto createDto)
    {
         var (status, dto) = await specificationService.AddExtensionElementAsync(specificationId, createDto);

         return status switch
         {
             ServiceResult.Success => TypedResults.Created($"/api/specifications/{specificationId}/extensionElements/{dto!.EntityID}", dto), // Assuming GetById exists
             ServiceResult.NotFound => TypedResults.NotFound(), // Parent spec not found
             ServiceResult.RefNotFound => TypedResults.BadRequest("Referenced Extension Component element not found."),
             _ => TypedResults.BadRequest("Failed to add extension element.")
         };
    }

    // PUT: api/specifications/5/extensionElements/12
    [HttpPut("{specificationId:int}/extensionElements/{extensionElementId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<NoContent, NotFound, BadRequest<string>>> PutSpecificationExtensionElement(
        int specificationId,
        int extensionElementId,
        [FromBody] SpecificationExtensionComponentUpdateDto updateDto)
    {
         var result = await specificationService.UpdateExtensionElementAsync(specificationId, extensionElementId, updateDto);

         return result switch
         {
             ServiceResult.Success => TypedResults.NoContent(),
             ServiceResult.NotFound => TypedResults.NotFound(), // Extension element within spec not found
             ServiceResult.RefNotFound => TypedResults.BadRequest("Referenced Extension Component element not found."),
             _ => TypedResults.BadRequest("Failed to update extension element.")
         };
    }

    // DELETE: api/specifications/5/extensionElements/12
    [HttpDelete("{specificationId:int}/extensionElements/{extensionElementId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
     public async Task<Results<NoContent, NotFound>> DeleteSpecificationExtensionElement(
        int specificationId,
        int extensionElementId)
    {
         var result = await specificationService.DeleteExtensionElementAsync(specificationId, extensionElementId);
         return result == ServiceResult.Success
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }
}
