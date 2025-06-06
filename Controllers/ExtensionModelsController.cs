using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using RegistryApi.DTOs;
using RegistryApi.Repositories;
using RegistryApi.Helpers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace RegistryApi.Controllers
{
    [Route("api/extensionmodels")]
    [ApiController]
    public class ExtensionModelsController : ControllerBase
    {
        private readonly IExtensionComponentsModelHeaderRepository _headerRepository;
        private readonly IExtensionComponentModelElementRepository _elementRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ExtensionModelsController> _logger;

        public ExtensionModelsController(
            IExtensionComponentsModelHeaderRepository headerRepository,
            IExtensionComponentModelElementRepository elementRepository,
            IMapper mapper,
            ILogger<ExtensionModelsController> logger)
        {
            _headerRepository = headerRepository;
            _elementRepository = elementRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/extensionmodels/headers
        [HttpGet("headers")]
        [ProducesResponseType<PaginatedExtensionComponentsModelHeaderResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<Results<Ok<PaginatedExtensionComponentsModelHeaderResponse>, BadRequest<string>>> GetExtensionComponentHeaders([FromQuery] PaginationParams paginationParams)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid pagination parameters received for GetExtensionComponentHeaders.");
                return TypedResults.BadRequest("Invalid pagination parameters.");
            }

            var pagedEntities = await _headerRepository.GetAllAsync(paginationParams);

            // Construct the PaginatedExtensionComponentsModelHeaderResponse
            var response = new PaginatedExtensionComponentsModelHeaderResponse(
                Metadata: new PaginationMetadata(
                    pagedEntities.TotalCount,
                    pagedEntities.PageSize,
                    pagedEntities.PageNumber,
                    pagedEntities.TotalPages,
                    pagedEntities.HasNextPage,
                    pagedEntities.HasPreviousPage
                ),
                Items: pagedEntities.Items // Items are already DTOs from the repository
            );

            return TypedResults.Ok(response);
        }

        // GET: api/extensionmodels/elements/{extensionComponentId}
        [HttpGet("elements/{extensionComponentId}")]
        [ProducesResponseType<PaginatedExtensionComponentModelElementResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<Results<Ok<PaginatedExtensionComponentModelElementResponse>, BadRequest<string>, NotFound>> GetExtensionComponentElements(
            string extensionComponentId,
            [FromQuery] PaginationParams paginationParams)
        {
            if (string.IsNullOrWhiteSpace(extensionComponentId))
            {
                _logger.LogWarning("ExtensionComponentId cannot be empty for GetExtensionComponentElements.");
                return TypedResults.BadRequest("ExtensionComponentId cannot be empty.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid pagination parameters received for GetExtensionComponentElements for ExtensionComponentId: {ExtensionComponentId}.", extensionComponentId);
                return TypedResults.BadRequest("Invalid pagination parameters.");
            }

            // Use the new GetByIdAsync(string id) from the header repository
            var headerExists = await _headerRepository.GetByIdAsync(extensionComponentId) != null;
            if (!headerExists)
            {
                _logger.LogInformation("ExtensionComponentsModelHeader with ID {ExtensionComponentId} not found when trying to fetch elements.", extensionComponentId);
                return TypedResults.NotFound();
            }

            var pagedElements = await _elementRepository.GetByExtensionComponentIdAsync(extensionComponentId, paginationParams);

            // Construct the PaginatedExtensionComponentModelElementResponse
            var response = new PaginatedExtensionComponentModelElementResponse(
                Metadata: new PaginationMetadata(
                    pagedElements.TotalCount,
                    pagedElements.PageSize,
                    pagedElements.PageNumber,
                    pagedElements.TotalPages,
                    pagedElements.HasNextPage,
                    pagedElements.HasPreviousPage
                ),
                Items: pagedElements.Items // Items are already DTOs from the repository
            );

            // If the header exists, but it has no elements, still return OK with an empty list.
            // This is generally preferred for list endpoints when the filter is valid.
            return TypedResults.Ok(response);
        }
    }
}