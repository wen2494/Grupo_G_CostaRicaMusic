using Grupo_G_WEB.Models;

namespace Grupo_G_WEB.Models.Api;

public class CatalogSearchResponse
{
    public string? Query { get; set; }
    public List<CancionDetalleDto> Canciones { get; set; } = [];
    public List<Album> Albumes { get; set; } = [];
    public List<Artista> Artistas { get; set; } = [];
}
