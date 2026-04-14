using Grupo_G_API.Models;
using Grupo_G_API.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Grupo_G_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NombreUsuario) || string.IsNullOrWhiteSpace(request.Contrasena))
        {
            return BadRequest(new { mensaje = "Usuario y contrasena son requeridos." });
        }

        var user = await authService.LoginAsync(request.NombreUsuario, request.Contrasena);
        return user is null ? Unauthorized(new { mensaje = "Usuario o contrasena incorrectos." }) : Ok(user);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NombreUsuario) || string.IsNullOrWhiteSpace(request.Contrasena))
        {
            return BadRequest(new { mensaje = "Usuario y contrasena son requeridos." });
        }

        var result = await authService.RegisterAsync(request);
        return result.Success ? Ok(result.User) : BadRequest(new { mensaje = result.Error });
    }
}
