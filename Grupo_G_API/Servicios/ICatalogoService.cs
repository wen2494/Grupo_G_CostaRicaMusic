using Grupo_G_API.Models;

namespace Grupo_G_API.Servicios;

public interface ICatalogoService
{
    Task<List<CancionDetalleDto>> BuscarCancionesAsync(string? query);
    Task<List<Album>> BuscarAlbumesAsync(string? query);
    Task<List<Artista>> BuscarArtistasAsync(string? query);
    Task<CancionDetalleDto?> ObtenerCancionPorIdAsync(int id);
    Task<Album?> ObtenerAlbumPorIdAsync(int id);
    Task<List<CancionDetalleDto>> ListarCancionesPorAlbumAsync(int idAlbum);
    Task<List<Playlist>> ListarPlaylistsPorUsuarioAsync(int idUsuario);
    Task<PlaylistDetalleDto?> ObtenerPlaylistPorIdAsync(int id);
    Task<Playlist> CrearPlaylistAsync(int idUsuario, string nombre, string? descripcion);
    Task<(bool Success, string? Error)> AgregarCancionAPlaylistAsync(int idPlaylist, int idCancion);
    Task<(bool Success, string? Error)> QuitarCancionDePlaylistAsync(int idPlaylist, int idCancion);
}
