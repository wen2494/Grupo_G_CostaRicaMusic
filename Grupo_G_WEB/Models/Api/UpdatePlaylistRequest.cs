namespace Grupo_G_WEB.Models.Api;

public class UpdatePlaylistRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
