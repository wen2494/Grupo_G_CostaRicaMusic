using Grupo_G_WEB.Models;
using Grupo_G_WEB.Models.Api;

namespace Grupo_G_WEB.Services;

public interface IMusicCatalogService
{
    Task<IReadOnlyList<Playlist>> GetPlaylistsAsync(int idUsuario = 1);
    Task<PlaylistDetalleDto?> GetPlaylistDetalleAsync(int playlistId);
    Task<Playlist> CreatePlaylistAsync(int idUsuario, string nombre, string? descripcion);
    Task<PlaylistMutationResult> AddSongToPlaylistAsync(int playlistId, int cancionId);
    Task<OperationResult> RemoveSongFromPlaylistAsync(int playlistId, int cancionId);

    Task<IReadOnlyList<CancionDetalleDto>> SearchSongsAsync(string? query);
    Task<IReadOnlyList<Album>> SearchAlbumsAsync(string? query);
    Task<IReadOnlyList<Artista>> SearchArtistsAsync(string? query);

    Task<CancionDetalleDto?> GetSongByIdAsync(int cancionId);
    Task<Artista?> GetArtistByIdAsync(int artistId);
    Task<IReadOnlyList<CancionDetalleDto>> GetSongsByArtistAsync(int artistId);
    Task<Album?> GetAlbumByIdAsync(int albumId);
    Task<IReadOnlyList<CancionDetalleDto>> GetSongsByAlbumAsync(int albumId);
}
