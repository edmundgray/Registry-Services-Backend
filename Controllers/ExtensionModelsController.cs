using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RegistryApi.DTOs;
using RegistryApi.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AutoMapper;

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
        [ProducesResponseType<IEnumerable<ExtensionComponentsModelHeaderDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ExtensionComponentsModelHeaderDto>>> GetExtensionComponentHeaders()
        {
            var headers = await _headerRepository.GetAllAsync();
            return Ok(headers);
        }

        // GET: api/extensionmodels/elements/{extensionComponentId}
        [HttpGet("elements/{extensionComponentId}")]
        [ProducesResponseType<IEnumerable<ExtensionComponentModelElementDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ExtensionComponentModelElementDto>>> GetExtensionComponentElements(string extensionComponentId)
        {
            if (string.IsNullOrWhiteSpace(extensionComponentId))
            {
                _logger.LogWarning("ExtensionComponentId cannot be empty for GetExtensionComponentElements.");
                return BadRequest("ExtensionComponentId cannot be empty.");
            }

            var headerExists = await _headerRepository.GetByIdAsync(extensionComponentId) != null;
            if (!headerExists)
            {
                _logger.LogInformation("ExtensionComponentsModelHeader with ID {ExtensionComponentId} not found when trying to fetch elements.", extensionComponentId);
                return NotFound();
            }

            var elements = await _elementRepository.GetByExtensionComponentIdAsync(extensionComponentId);
            return Ok(elements);
        }
    }
}
