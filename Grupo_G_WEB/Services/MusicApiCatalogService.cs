using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Grupo_G_WEB.Models;
using Grupo_G_WEB.Models.Api;

namespace Grupo_G_WEB.Services;

public class MusicApiCatalogService(HttpClient httpClient) : IMusicCatalogService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient = httpClient;

    public async Task<IReadOnlyList<Playlist>> GetPlaylistsAsync(int idUsuario = 1)
        => await _httpClient.GetFromJsonAsync<List<Playlist>>($"/api/playlists?idUsuario={idUsuario}", JsonOptions) ?? [];

    public async Task<PlaylistDetalleDto?> GetPlaylistDetalleAsync(int playlistId)
    {
        var response = await _httpClient.GetAsync($"/api/playlists/{playlistId}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlaylistDetalleDto>(JsonOptions);
    }

    public async Task<Playlist> CreatePlaylistAsync(int idUsuario, string nombre, string? descripcion)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/playlists", new CreatePlaylistRequest
        {
            IdUsuario = idUsuario,
            Nombre = nombre,
            Descripcion = descripcion
        }, JsonOptions);

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Playlist>(JsonOptions))!;
    }

    public async Task<PlaylistMutationResult> UpdatePlaylistAsync(int playlistId, string nombre, string? descripcion)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/playlists/{playlistId}", new UpdatePlaylistRequest
        {
            Nombre = nombre,
            Descripcion = descripcion
        }, JsonOptions);

        if (!response.IsSuccessStatusCode)
        {
            return new PlaylistMutationResult(false, await ReadErrorMessageAsync(response));
        }

        var playlist = await GetPlaylistDetalleAsync(playlistId);
        return new PlaylistMutationResult(true, null, playlist);
    }

    public async Task<OperationResult> DeletePlaylistAsync(int playlistId)
    {
        var response = await _httpClient.DeleteAsync($"/api/playlists/{playlistId}");
        return response.IsSuccessStatusCode
            ? new OperationResult(true)
            : new OperationResult(false, await ReadErrorMessageAsync(response));
    }

    public async Task<PlaylistMutationResult> AddSongToPlaylistAsync(int playlistId, int cancionId)
    {
        var response = await _httpClient.PostAsJsonAsync($"/api/playlists/{playlistId}/canciones", new AddSongToPlaylistRequest
        {
            IdCancion = cancionId
        }, JsonOptions);

        if (!response.IsSuccessStatusCode)
        {
            return new PlaylistMutationResult(false, await ReadErrorMessageAsync(response));
        }

        var playlist = await response.Content.ReadFromJsonAsync<PlaylistDetalleDto>(JsonOptions);
        return new PlaylistMutationResult(true, null, playlist);
    }

    public async Task<OperationResult> RemoveSongFromPlaylistAsync(int playlistId, int cancionId)
    {
        var response = await _httpClient.DeleteAsync($"/api/playlists/{playlistId}/canciones/{cancionId}");
        return response.IsSuccessStatusCode
            ? new OperationResult(true)
            : new OperationResult(false, await ReadErrorMessageAsync(response));
    }

    public async Task<IReadOnlyList<CancionDetalleDto>> SearchSongsAsync(string? query)
        => (await SearchAsync(query, "canciones")).Canciones;

    public async Task<IReadOnlyList<Album>> SearchAlbumsAsync(string? query)
        => (await SearchAsync(query, "albumes")).Albumes;

    public async Task<IReadOnlyList<Artista>> SearchArtistsAsync(string? query)
        => (await SearchAsync(query, "artistas")).Artistas;

    public async Task<CancionDetalleDto?> GetSongByIdAsync(int cancionId)
    {
        var response = await _httpClient.GetAsync($"/api/catalogo/canciones/{cancionId}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CancionDetalleDto>(JsonOptions);
    }

    public async Task<Artista?> GetArtistByIdAsync(int artistId)
    {
        var response = await _httpClient.GetAsync($"/api/artista/{artistId}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Artista>(JsonOptions);
    }

    public async Task<IReadOnlyList<CancionDetalleDto>> GetSongsByArtistAsync(int artistId)
    {
        var response = await _httpClient.GetAsync($"/api/artista/{artistId}/canciones");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return [];
        }

        response.EnsureSuccessStatusCode();
        var artist = await GetArtistByIdAsync(artistId);
        var songs = await response.Content.ReadFromJsonAsync<List<ArtistSongResponse>>(JsonOptions) ?? [];

        return songs.Select(song => new CancionDetalleDto
        {
            Id = song.Id,
            NombreCancion = song.NombreCancion,
            DuracionSegundos = song.DuracionSegundos,
            NumeroPista = song.NumeroPista,
            RutaArchivo = song.RutaArchivo,
            IdArtista = artistId,
            NombreArtista = song.NombreArtista ?? artist?.Nombre ?? string.Empty,
            BiografiaArtista = artist?.Biografia,
            UrlImagenArtista = artist?.UrlImagen,
            IdAlbum = song.IdAlbum,
            NombreAlbum = song.NombreAlbum,
            AnioAlbum = song.Anio
        }).ToList();
    }

    public async Task<Album?> GetAlbumByIdAsync(int albumId)
    {
        var response = await _httpClient.GetAsync($"/api/catalogo/albumes/{albumId}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Album>(JsonOptions);
    }

    public async Task<IReadOnlyList<CancionDetalleDto>> GetSongsByAlbumAsync(int albumId)
        => await _httpClient.GetFromJsonAsync<List<CancionDetalleDto>>($"/api/catalogo/albumes/{albumId}/canciones", JsonOptions) ?? [];

    private async Task<CatalogSearchResponse> SearchAsync(string? query, string tipo)
    {
        var q = string.IsNullOrWhiteSpace(query) ? string.Empty : $"&q={Uri.EscapeDataString(query.Trim())}";
        return await _httpClient.GetFromJsonAsync<CatalogSearchResponse>($"/api/catalogo/search?tipo={tipo}{q}", JsonOptions)
            ?? new CatalogSearchResponse();
    }

    private static async Task<string?> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
        return payload?.Mensaje ?? $"{(int)response.StatusCode} {response.ReasonPhrase}";
    }

    private sealed class ArtistSongResponse
    {
        public int Id { get; set; }
        public string NombreCancion { get; set; } = string.Empty;
        public int DuracionSegundos { get; set; }
        public int NumeroPista { get; set; }
        public string RutaArchivo { get; set; } = string.Empty;
        public string? NombreArtista { get; set; }
        public int IdAlbum { get; set; }
        public string NombreAlbum { get; set; } = string.Empty;
        public int? Anio { get; set; }
    }
}
