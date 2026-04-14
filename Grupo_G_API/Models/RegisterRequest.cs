namespace Grupo_G_API.Models
{
    public class RegisterRequest
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? NombreCompleto { get; set; }
    }
}
