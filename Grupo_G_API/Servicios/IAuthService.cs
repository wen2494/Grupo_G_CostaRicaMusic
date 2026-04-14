using Grupo_G_API.Models;

namespace Grupo_G_API.Servicios;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string nombreUsuario, string contrasena);
    Task<(bool Success, string? Error, LoginResponse? User)> RegisterAsync(RegisterRequest request);
}
