using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TMS.Application;
using TMS.Application.Features;

namespace TMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Authentication and Account Management")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "User Login",
            Description = "Authenticates a user using email and password. Returns JWT tokens and the authenticated user profile.",
            OperationId = "Auth.Login"
        )]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _mediator.Send(new LoginCommand(request));
            return ApiResponse.FromResult(this, result);
        }


        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "User Registration",
            Description = "Creates a new user account. Returns JWT tokens and the registered user profile.",
            OperationId = "Auth.Register"
        )]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _mediator.Send(new RegisterUserCommand(request));
            return ApiResponse.FromResult(this, result);
        }


        [HttpPost("refresh-token")]
        [SwaggerOperation(
            Summary = "Refresh Access Token",
            Description = "Issues new JWT and refresh tokens using a valid (possibly expired) access token and refresh token.",
            OperationId = "Auth.RefreshToken"
        )]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _mediator.Send(new RefreshTokenCommand(request));
            return ApiResponse.FromResult(this, result);
        }
    }
}
