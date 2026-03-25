namespace Grupo_G_API.Models;

public class CatalogDataFile
{
    public List<Artista> Artistas { get; set; } = [];
    public List<Album> Albumes { get; set; } = [];
    public List<Cancion> Canciones { get; set; } = [];
    public List<Playlist> Playlists { get; set; } = [];
    public List<PlaylistCancion> PlaylistCanciones { get; set; } = [];
}
