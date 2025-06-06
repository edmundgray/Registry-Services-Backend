using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // For StatusCodes
using Microsoft.AspNetCore.Http.HttpResults; // For TypedResults
using RegistryApi.DTOs;
using RegistryApi.Repositories;
using RegistryApi.Helpers; // For PaginationParams
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AutoMapper; // For IMapper
using Microsoft.AspNetCore.Authorization; // For [Authorize]

namespace RegistryApi.Controllers
{
    [Route("api/coreinvoicemodels")]
    [ApiController]
    
    public class CoreInvoiceModelsController : ControllerBase
    {
        private readonly ICoreInvoiceModelRepository _coreInvoiceModelRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CoreInvoiceModelsController> _logger;


        public CoreInvoiceModelsController(ICoreInvoiceModelRepository coreInvoiceModelRepository, IMapper mapper, ILogger<CoreInvoiceModelsController> logger)
        {
            _coreInvoiceModelRepository = coreInvoiceModelRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/coreinvoicemodels
        [HttpGet]
        [ProducesResponseType<PagedList<CoreInvoiceModelDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<Results<Ok<PagedList<CoreInvoiceModelDto>>, BadRequest<string>>> GetCoreInvoiceModels([FromQuery] PaginationParams paginationParams)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid pagination parameters received for GetCoreInvoiceModels.");
                return TypedResults.BadRequest("Invalid pagination parameters.");
            }

            var coreModels = await _coreInvoiceModelRepository.GetAllAsync(paginationParams);
            return TypedResults.Ok(coreModels);
        }
    }
}