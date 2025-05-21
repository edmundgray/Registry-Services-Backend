using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults; // For TypedResults
using RegistryApi.DTOs;
using RegistryApi.Services;
using System.Threading.Tasks;

namespace RegistryApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [ProducesResponseType<UserTokenDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // For validation errors
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<Results<Ok<UserTokenDto>, BadRequest<string>, UnauthorizedHttpResult>> Login([FromBody] UserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return TypedResults.BadRequest("Invalid login request data.");
            }

            var (authStatus, tokenDto, errorMessage) = await _userService.AuthenticateUserAsync(loginDto);

            return authStatus switch
            {
                ServiceResult.Success => TypedResults.Ok(tokenDto!),
                ServiceResult.Unauthorized => TypedResults.Unauthorized(),
                _ => TypedResults.BadRequest(errorMessage ?? "Login failed for an unknown reason.")
            };
        }
    }
}
