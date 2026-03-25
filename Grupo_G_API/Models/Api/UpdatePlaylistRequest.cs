namespace Grupo_G_API.Models.Api;

public class UpdatePlaylistRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
