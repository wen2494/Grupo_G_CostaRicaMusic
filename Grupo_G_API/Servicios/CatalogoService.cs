using Grupo_G_API.Models;

namespace Grupo_G_API.Servicios;

public class CatalogoService(IJsonCatalogStore store) : ICatalogoService
{
    public async Task<List<CancionDetalleDto>> BuscarCancionesAsync(string? query)
    {
        var data = await store.ReadAsync();
        var normalized = query?.Trim();

        return data.Canciones
            .Where(song => string.IsNullOrWhiteSpace(normalized) || song.Nombre.Contains(normalized, StringComparison.OrdinalIgnoreCase))
            .Select(song => MapSong(song, data))
            .OrderBy(song => song.NombreCancion)
            .ToList();
    }

    public async Task<List<Album>> BuscarAlbumesAsync(string? query)
    {
        var data = await store.ReadAsync();
        var normalized = query?.Trim();

        return data.Albumes
            .Where(album => string.IsNullOrWhiteSpace(normalized) || album.Nombre.Contains(normalized, StringComparison.OrdinalIgnoreCase))
            .OrderBy(album => album.Nombre)
            .ToList();
    }

    public async Task<List<Artista>> BuscarArtistasAsync(string? query)
    {
        var data = await store.ReadAsync();
        var normalized = query?.Trim();

        return data.Artistas
            .Where(artist => string.IsNullOrWhiteSpace(normalized) || artist.Nombre.Contains(normalized, StringComparison.OrdinalIgnoreCase))
            .OrderBy(artist => artist.Nombre)
            .ToList();
    }

    public async Task<CancionDetalleDto?> ObtenerCancionPorIdAsync(int id)
    {
        var data = await store.ReadAsync();
        var song = data.Canciones.FirstOrDefault(item => item.Id == id);
        return song is null ? null : MapSong(song, data);
    }

    public async Task<Album?> ObtenerAlbumPorIdAsync(int id)
    {
        var data = await store.ReadAsync();
        return data.Albumes.FirstOrDefault(album => album.Id == id);
    }

    public async Task<List<CancionDetalleDto>> ListarCancionesPorAlbumAsync(int idAlbum)
    {
        var data = await store.ReadAsync();

        return data.Canciones
            .Where(song => song.IdAlbum == idAlbum)
            .OrderBy(song => song.NumeroPista)
            .ThenBy(song => song.Nombre)
            .Select(song => MapSong(song, data))
            .ToList();
    }

    public async Task<List<Playlist>> ListarPlaylistsPorUsuarioAsync(int idUsuario)
    {
        var data = await store.ReadAsync();

        return data.Playlists
            .Where(playlist => playlist.IdUsuario == idUsuario)
            .OrderByDescending(playlist => playlist.FechaCreacion)
            .ToList();
    }

    public async Task<PlaylistDetalleDto?> ObtenerPlaylistPorIdAsync(int id)
    {
        var data = await store.ReadAsync();
        var playlist = data.Playlists.FirstOrDefault(item => item.Id == id);
        if (playlist is null)
        {
            return null;
        }

        return BuildPlaylistDetail(playlist, data);
    }

    public async Task<Playlist> CrearPlaylistAsync(int idUsuario, string nombre, string? descripcion)
    {
        var data = await store.ReadAsync();
        var nextId = data.Playlists.Count == 0 ? 1 : data.Playlists.Max(item => item.Id) + 1;

        var playlist = new Playlist
        {
            Id = nextId,
            IdUsuario = idUsuario,
            Nombre = nombre,
            Descripcion = descripcion,
            FechaCreacion = DateTime.UtcNow
        };

        data.Playlists.Add(playlist);
        await store.WriteAsync(data);
        return playlist;
    }

    public async Task<(bool Success, string? Error, Playlist? Playlist)> ActualizarPlaylistAsync(int id, string nombre, string? descripcion)
    {
        var data = await store.ReadAsync();
        var playlist = data.Playlists.FirstOrDefault(item => item.Id == id);
        if (playlist is null)
        {
            return (false, "La playlist no existe.", null);
        }

        playlist.Nombre = nombre;
        playlist.Descripcion = descripcion;
        await store.WriteAsync(data);
        return (true, null, playlist);
    }

    public async Task<(bool Success, string? Error)> EliminarPlaylistAsync(int id)
    {
        var data = await store.ReadAsync();
        var playlist = data.Playlists.FirstOrDefault(item => item.Id == id);
        if (playlist is null)
        {
            return (false, "La playlist no existe.");
        }

        data.Playlists.Remove(playlist);
        data.PlaylistCanciones.RemoveAll(item => item.IdPlaylist == id);
        await store.WriteAsync(data);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> AgregarCancionAPlaylistAsync(int idPlaylist, int idCancion)
    {
        var data = await store.ReadAsync();

        if (!data.Playlists.Any(item => item.Id == idPlaylist))
        {
            return (false, "La playlist no existe.");
        }

        if (!data.Canciones.Any(item => item.Id == idCancion))
        {
            return (false, "La cancion no existe.");
        }

        if (data.PlaylistCanciones.Any(item => item.IdPlaylist == idPlaylist && item.IdCancion == idCancion))
        {
            return (false, "La cancion ya esta en la playlist.");
        }

        var nextId = data.PlaylistCanciones.Count == 0 ? 1 : data.PlaylistCanciones.Max(item => item.Id) + 1;
        var nextOrder = data.PlaylistCanciones
            .Where(item => item.IdPlaylist == idPlaylist)
            .Select(item => item.Orden)
            .DefaultIfEmpty(0)
            .Max() + 1;

        data.PlaylistCanciones.Add(new PlaylistCancion
        {
            Id = nextId,
            IdPlaylist = idPlaylist,
            IdCancion = idCancion,
            Orden = nextOrder,
            FechaAgregado = DateTime.UtcNow
        });

        await store.WriteAsync(data);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> QuitarCancionDePlaylistAsync(int idPlaylist, int idCancion)
    {
        var data = await store.ReadAsync();
        var relation = data.PlaylistCanciones.FirstOrDefault(item => item.IdPlaylist == idPlaylist && item.IdCancion == idCancion);
        if (relation is null)
        {
            return (false, "La cancion no esta en la playlist indicada.");
        }

        data.PlaylistCanciones.Remove(relation);
        await store.WriteAsync(data);
        return (true, null);
    }

    private static PlaylistDetalleDto BuildPlaylistDetail(Playlist playlist, CatalogDataFile data)
    {
        var songs = data.PlaylistCanciones
            .Where(item => item.IdPlaylist == playlist.Id)
            .OrderBy(item => item.Orden)
            .ThenBy(item => item.FechaAgregado)
            .Select(item =>
            {
                var song = data.Canciones.FirstOrDefault(track => track.Id == item.IdCancion);
                if (song is null)
                {
                    return null;
                }

                var artist = data.Artistas.First(trackArtist => trackArtist.Id == song.IdArtista);
                var album = data.Albumes.First(trackAlbum => trackAlbum.Id == song.IdAlbum);

                return new CancionEnPlaylistDto
                {
                    Id = item.Id,
                    Orden = item.Orden,
                    FechaAgregado = item.FechaAgregado,
                    IdCancion = song.Id,
                    NombreCancion = song.Nombre,
                    DuracionSegundos = song.DuracionSegundos,
                    RutaArchivo = song.RutaArchivo,
                    IdArtista = artist.Id,
                    NombreArtista = artist.Nombre,
                    IdAlbum = album.Id,
                    NombreAlbum = album.Nombre,
                    UrlPortadaAlbum = album.UrlPortada
                };
            })
            .Where(item => item is not null)
            .Cast<CancionEnPlaylistDto>()
            .ToList();

        return new PlaylistDetalleDto
        {
            Id = playlist.Id,
            IdUsuario = playlist.IdUsuario,
            Nombre = playlist.Nombre,
            Descripcion = playlist.Descripcion,
            FechaCreacion = playlist.FechaCreacion,
            Canciones = songs
        };
    }

    private static CancionDetalleDto MapSong(Cancion song, CatalogDataFile data)
    {
        var artist = data.Artistas.First(item => item.Id == song.IdArtista);
        var album = data.Albumes.First(item => item.Id == song.IdAlbum);

        return new CancionDetalleDto
        {
            Id = song.Id,
            NombreCancion = song.Nombre,
            DuracionSegundos = song.DuracionSegundos,
            NumeroPista = song.NumeroPista,
            RutaArchivo = song.RutaArchivo,
            FechaCreacion = song.FechaCreacion,
            IdArtista = artist.Id,
            NombreArtista = artist.Nombre,
            BiografiaArtista = artist.Biografia,
            UrlImagenArtista = artist.UrlImagen,
            IdAlbum = album.Id,
            NombreAlbum = album.Nombre,
            AnioAlbum = album.Anio,
            UrlPortadaAlbum = album.UrlPortada
        };
    }
}
