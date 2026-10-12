using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Matoli.Api.Models;
using Matoli.Api.Services;

namespace Matoli.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Debe proporcionar usuario y contraseña" });
        }

        if (!_authService.ValidateCredentials(request.Username, request.Password))
        {
            return Unauthorized(new { error = "Usuario o contraseña incorrectos" });
        }

        var response = _authService.GenerateToken(request.Username);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("verify")]
    public IActionResult Verify()
    {
        var username = User.Identity?.Name ?? "Usuario";
        return Ok(new { valid = true, user = username });
    }
}
