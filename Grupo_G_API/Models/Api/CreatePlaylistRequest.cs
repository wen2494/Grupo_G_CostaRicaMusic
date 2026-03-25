namespace Grupo_G_API.Models.Api;

public class CreatePlaylistRequest
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
