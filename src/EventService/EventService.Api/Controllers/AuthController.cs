using EventService.Api.Auth;
using Microsoft.AspNetCore.Mvc;

namespace EventService.Api.Controllers;

/// <summary>
/// Emisor de tokens para la demo. Permite obtener un JWT con rol Admin o User
/// sin necesitar un IdP externo. Endpoint sin autenticación (es el punto de entrada).
/// </summary>
[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly JwtTokenService _tokenService;

    public AuthController(JwtTokenService tokenService) => _tokenService = tokenService;

    [HttpPost("token")]
    public ActionResult<object> GenerateToken([FromBody] TokenRequest request)
    {
        if (request.Role is not ("Admin" or "User"))
            return BadRequest(new { title = "El rol debe ser 'Admin' o 'User'." });

        var token = _tokenService.GenerateToken(request.Username, request.Role);
        return Ok(new { accessToken = token, expiresInMinutes = 120 });
    }
}
