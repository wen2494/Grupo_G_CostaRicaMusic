using System.Security.Cryptography;
using System.Text;
using Grupo_G_API.Models;

namespace Grupo_G_API.Servicios;

public class AuthService(IJsonCatalogStore store) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(string nombreUsuario, string contrasena)
    {
        var data = await store.ReadAsync();
        var normalized = nombreUsuario.Trim();
        var passwordHash = HashPassword(contrasena);

        var user = data.Usuarios.FirstOrDefault(item =>
            item.Activo &&
            item.NombreUsuario.Equals(normalized, StringComparison.OrdinalIgnoreCase) &&
            item.ContrasenaHash.Equals(passwordHash, StringComparison.OrdinalIgnoreCase));

        if (user is null)
        {
            return null;
        }

        return ToLoginResponse(user);
    }

    public async Task<(bool Success, string? Error, LoginResponse? User)> RegisterAsync(RegisterRequest request)
    {
        var normalizedUserName = request.NombreUsuario.Trim();
        var normalizedEmail = request.Email?.Trim();

        if (normalizedUserName.Length < 3)
        {
            return (false, "El usuario debe tener al menos 3 caracteres.", null);
        }

        if (request.Contrasena.Length < 4)
        {
            return (false, "La contrasena debe tener al menos 4 caracteres.", null);
        }

        var data = await store.ReadAsync();
        if (data.Usuarios.Any(item => item.NombreUsuario.Equals(normalizedUserName, StringComparison.OrdinalIgnoreCase)))
        {
            return (false, "Ese usuario ya existe.", null);
        }

        if (!string.IsNullOrWhiteSpace(normalizedEmail) &&
            data.Usuarios.Any(item => item.Email?.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase) == true))
        {
            return (false, "Ese correo ya esta en uso.", null);
        }

        var user = new Usuario
        {
            Id = data.Usuarios.Count == 0 ? 1 : data.Usuarios.Max(item => item.Id) + 1,
            NombreUsuario = normalizedUserName,
            Email = string.IsNullOrWhiteSpace(normalizedEmail) ? null : normalizedEmail,
            ContrasenaHash = HashPassword(request.Contrasena),
            NombreCompleto = string.IsNullOrWhiteSpace(request.NombreCompleto) ? normalizedUserName : request.NombreCompleto.Trim(),
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        data.Usuarios.Add(user);
        await store.WriteAsync(data);

        return (true, null, ToLoginResponse(user));
    }

    private static LoginResponse ToLoginResponse(Usuario user)
    {
        return new LoginResponse
        {
            Id = user.Id,
            NombreUsuario = user.NombreUsuario,
            Email = user.Email,
            NombreCompleto = user.NombreCompleto,
            Token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.Id}:{user.NombreUsuario}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"))
        };
    }

    private static string HashPassword(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
