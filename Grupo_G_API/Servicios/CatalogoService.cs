using Grupo_G_API.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Grupo_G_API.Servicios;

public class CatalogoService(IConfiguration configuration) : ICatalogoService
{
    private const string FallbackAudioBase = "https://www.soundhelix.com/examples/mp3/";
    private readonly IConfiguration _configuration = configuration;

    private string ConnectionString => _configuration.GetConnectionString("BDConnection") ?? string.Empty;

    public async Task<List<CancionDetalleDto>> BuscarCancionesAsync(string? query)
    {
        const string sql = """
            SELECT c.Id, c.Nombre AS NombreCancion, c.DuracionSegundos, c.NumeroPista, c.RutaArchivo, c.FechaCreacion,
                   a.Id AS IdArtista, a.Nombre AS NombreArtista, a.Biografia AS BiografiaArtista, a.UrlImagen AS UrlImagenArtista,
                   al.Id AS IdAlbum, al.Nombre AS NombreAlbum, al.Anio AS AnioAlbum, al.UrlPortada AS UrlPortadaAlbum
            FROM Canciones c
            INNER JOIN Artistas a ON c.IdArtista = a.Id
            INNER JOIN Albumes al ON c.IdAlbum = al.Id
            WHERE (@Query IS NULL OR @Query = '' OR c.Nombre LIKE N'%' + @Query + N'%')
            ORDER BY c.Nombre;
            """;

        return await ReadSongsAsync(sql, command =>
        {
            command.Parameters.AddWithValue("@Query", (object?)query?.Trim() ?? DBNull.Value);
        });
    }

    public async Task<List<Album>> BuscarAlbumesAsync(string? query)
    {
        const string sql = """
            SELECT Id, Nombre, IdArtista, Anio, UrlPortada, FechaCreacion
            FROM Albumes
            WHERE (@Query IS NULL OR @Query = '' OR Nombre LIKE N'%' + @Query + N'%')
            ORDER BY Nombre;
            """;

        var items = new List<Album>();
        await using var cn = new SqlConnection(ConnectionString);
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Query", (object?)query?.Trim() ?? DBNull.Value);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new Album
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                IdArtista = reader.GetInt32(2),
                Anio = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                UrlPortada = reader.IsDBNull(4) ? null : reader.GetString(4),
                FechaCreacion = reader.GetDateTime(5)
            });
        }

        return items;
    }

    public async Task<List<Artista>> BuscarArtistasAsync(string? query)
    {
        const string sql = """
            SELECT Id, Nombre, Biografia, UrlImagen, FechaCreacion
            FROM Artistas
            WHERE (@Query IS NULL OR @Query = '' OR Nombre LIKE N'%' + @Query + N'%')
            ORDER BY Nombre;
            """;

        var items = new List<Artista>();
        await using var cn = new SqlConnection(ConnectionString);
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Query", (object?)query?.Trim() ?? DBNull.Value);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new Artista
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Biografia = reader.IsDBNull(2) ? null : reader.GetString(2),
                UrlImagen = reader.IsDBNull(3) ? null : reader.GetString(3),
                FechaCreacion = reader.GetDateTime(4)
            });
        }

        return items;
    }

    public async Task<CancionDetalleDto?> ObtenerCancionPorIdAsync(int id)
    {
        const string sql = """
            SELECT c.Id, c.Nombre AS NombreCancion, c.DuracionSegundos, c.NumeroPista, c.RutaArchivo, c.FechaCreacion,
                   a.Id AS IdArtista, a.Nombre AS NombreArtista, a.Biografia AS BiografiaArtista, a.UrlImagen AS UrlImagenArtista,
                   al.Id AS IdAlbum, al.Nombre AS NombreAlbum, al.Anio AS AnioAlbum, al.UrlPortada AS UrlPortadaAlbum
            FROM Canciones c
            INNER JOIN Artistas a ON c.IdArtista = a.Id
            INNER JOIN Albumes al ON c.IdAlbum = al.Id
            WHERE c.Id = @Id;
            """;

        var songs = await ReadSongsAsync(sql, command => command.Parameters.AddWithValue("@Id", id));
        return songs.FirstOrDefault();
    }

    public async Task<Album?> ObtenerAlbumPorIdAsync(int id)
    {
        const string sql = """
            SELECT Id, Nombre, IdArtista, Anio, UrlPortada, FechaCreacion
            FROM Albumes
            WHERE Id = @Id;
            """;

        await using var cn = new SqlConnection(ConnectionString);
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Album
        {
            Id = reader.GetInt32(0),
            Nombre = reader.GetString(1),
            IdArtista = reader.GetInt32(2),
            Anio = reader.IsDBNull(3) ? null : reader.GetInt32(3),
            UrlPortada = reader.IsDBNull(4) ? null : reader.GetString(4),
            FechaCreacion = reader.GetDateTime(5)
        };
    }

    public async Task<List<CancionDetalleDto>> ListarCancionesPorAlbumAsync(int idAlbum)
    {
        const string sql = """
            SELECT c.Id, c.Nombre AS NombreCancion, c.DuracionSegundos, c.NumeroPista, c.RutaArchivo, c.FechaCreacion,
                   a.Id AS IdArtista, a.Nombre AS NombreArtista, a.Biografia AS BiografiaArtista, a.UrlImagen AS UrlImagenArtista,
                   al.Id AS IdAlbum, al.Nombre AS NombreAlbum, al.Anio AS AnioAlbum, al.UrlPortada AS UrlPortadaAlbum
            FROM Canciones c
            INNER JOIN Artistas a ON c.IdArtista = a.Id
            INNER JOIN Albumes al ON c.IdAlbum = al.Id
            WHERE c.IdAlbum = @IdAlbum
            ORDER BY c.NumeroPista, c.Nombre;
            """;

        return await ReadSongsAsync(sql, command => command.Parameters.AddWithValue("@IdAlbum", idAlbum));
    }

    public async Task<List<Playlist>> ListarPlaylistsPorUsuarioAsync(int idUsuario)
    {
        const string sql = """
            SELECT Id, IdUsuario, Nombre, Descripcion, FechaCreacion
            FROM Playlists
            WHERE IdUsuario = @IdUsuario
            ORDER BY FechaCreacion DESC;
            """;

        var items = new List<Playlist>();
        await using var cn = new SqlConnection(ConnectionString);
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new Playlist
            {
                Id = reader.GetInt32(0),
                IdUsuario = reader.GetInt32(1),
                Nombre = reader.GetString(2),
                Descripcion = reader.IsDBNull(3) ? null : reader.GetString(3),
                FechaCreacion = reader.GetDateTime(4)
            });
        }

        return items;
    }

    public async Task<PlaylistDetalleDto?> ObtenerPlaylistPorIdAsync(int id)
    {
        const string playlistSql = """
            SELECT Id, IdUsuario, Nombre, Descripcion, FechaCreacion
            FROM Playlists
            WHERE Id = @Id;
            """;

        const string cancionesSql = """
            SELECT pc.Id, pc.Orden, pc.FechaAgregado,
                   c.Id AS IdCancion, c.Nombre AS NombreCancion, c.DuracionSegundos, c.RutaArchivo,
                   a.Id AS IdArtista, a.Nombre AS NombreArtista,
                   al.Id AS IdAlbum, al.Nombre AS NombreAlbum
            FROM PlaylistCanciones pc
            INNER JOIN Canciones c ON pc.IdCancion = c.Id
            INNER JOIN Artistas a ON c.IdArtista = a.Id
            INNER JOIN Albumes al ON c.IdAlbum = al.Id
            WHERE pc.IdPlaylist = @Id
            ORDER BY pc.Orden, pc.FechaAgregado;
            """;

        await using var cn = new SqlConnection(ConnectionString);
        await cn.OpenAsync();

        PlaylistDetalleDto? playlist = null;
        await using (var cmd = new SqlCommand(playlistSql, cn))
        {
            cmd.Parameters.AddWithValue("@Id", id);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                playlist = new PlaylistDetalleDto
                {
                    Id = reader.GetInt32(0),
                    IdUsuario = reader.GetInt32(1),
                    Nombre = reader.GetString(2),
                    Descripcion = reader.IsDBNull(3) ? null : reader.GetString(3),
                    FechaCreacion = reader.GetDateTime(4)
                };
            }
        }

        if (playlist is null)
        {
            return null;
        }

        await using (var cmd = new SqlCommand(cancionesSql, cn))
        {
            cmd.Parameters.AddWithValue("@Id", id);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                playlist.Canciones.Add(new CancionEnPlaylistDto
                {
                    Id = reader.GetInt32(0),
                    Orden = reader.GetInt32(1),
                    FechaAgregado = reader.GetDateTime(2),
                    IdCancion = reader.GetInt32(3),
                    NombreCancion = reader.GetString(4),
                    DuracionSegundos = reader.GetInt32(5),
                    RutaArchivo = NormalizePlaybackUrl(reader.GetString(6), reader.GetInt32(3)),
                    IdArtista = reader.GetInt32(7),
                    NombreArtista = reader.GetString(8),
                    IdAlbum = reader.GetInt32(9),
                    NombreAlbum = reader.GetString(10)
                });
            }
        }

        return playlist;
    }

    public async Task<Playlist> CrearPlaylistAsync(int idUsuario, string nombre, string? descripcion)
    {
        const string sql = """
            INSERT INTO Playlists (IdUsuario, Nombre, Descripcion)
            OUTPUT INSERTED.Id, INSERTED.IdUsuario, INSERTED.Nombre, INSERTED.Descripcion, INSERTED.FechaCreacion
            VALUES (@IdUsuario, @Nombre, @Descripcion);
            """;

        await using var cn = new SqlConnection(ConnectionString);
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
        cmd.Parameters.AddWithValue("@Nombre", nombre);
        cmd.Parameters.AddWithValue("@Descripcion", (object?)descripcion ?? DBNull.Value);
        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
        await reader.ReadAsync();

        return new Playlist
        {
            Id = reader.GetInt32(0),
            IdUsuario = reader.GetInt32(1),
            Nombre = reader.GetString(2),
            Descripcion = reader.IsDBNull(3) ? null : reader.GetString(3),
            FechaCreacion = reader.GetDateTime(4)
        };
    }

    public async Task<(bool Success, string? Error)> AgregarCancionAPlaylistAsync(int idPlaylist, int idCancion)
    {
        await using var cn = new SqlConnection(ConnectionString);
        await cn.OpenAsync();

        if (!await ExisteAsync(cn, "SELECT COUNT(1) FROM Playlists WHERE Id = @Id", "@Id", idPlaylist))
        {
            return (false, "La playlist no existe.");
        }

        if (!await ExisteAsync(cn, "SELECT COUNT(1) FROM Canciones WHERE Id = @Id", "@Id", idCancion))
        {
            return (false, "La cancion no existe.");
        }

        if (await ExisteAsync(cn, "SELECT COUNT(1) FROM PlaylistCanciones WHERE IdPlaylist = @IdPlaylist AND IdCancion = @IdCancion", ("@IdPlaylist", idPlaylist), ("@IdCancion", idCancion)))
        {
            return (false, "La cancion ya esta en la playlist.");
        }

        const string sql = """
            DECLARE @Orden INT = ISNULL((SELECT MAX(Orden) + 1 FROM PlaylistCanciones WHERE IdPlaylist = @IdPlaylist), 1);
            INSERT INTO PlaylistCanciones (IdPlaylist, IdCancion, Orden)
            VALUES (@IdPlaylist, @IdCancion, @Orden);
            """;

        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@IdPlaylist", idPlaylist);
        cmd.Parameters.AddWithValue("@IdCancion", idCancion);
        await cmd.ExecuteNonQueryAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> QuitarCancionDePlaylistAsync(int idPlaylist, int idCancion)
    {
        const string sql = """
            DELETE FROM PlaylistCanciones
            WHERE IdPlaylist = @IdPlaylist AND IdCancion = @IdCancion;
            """;

        await using var cn = new SqlConnection(ConnectionString);
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@IdPlaylist", idPlaylist);
        cmd.Parameters.AddWithValue("@IdCancion", idCancion);
        var affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0
            ? (true, null)
            : (false, "La cancion no esta en la playlist indicada.");
    }

    private async Task<List<CancionDetalleDto>> ReadSongsAsync(string sql, Action<SqlCommand> configureCommand)
    {
        var songs = new List<CancionDetalleDto>();
        await using var cn = new SqlConnection(ConnectionString);
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        configureCommand(cmd);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            songs.Add(new CancionDetalleDto
            {
                Id = reader.GetInt32(0),
                NombreCancion = reader.GetString(1),
                DuracionSegundos = reader.GetInt32(2),
                NumeroPista = reader.GetInt32(3),
                RutaArchivo = NormalizePlaybackUrl(reader.GetString(4), reader.GetInt32(0)),
                FechaCreacion = reader.GetDateTime(5),
                IdArtista = reader.GetInt32(6),
                NombreArtista = reader.GetString(7),
                BiografiaArtista = reader.IsDBNull(8) ? null : reader.GetString(8),
                UrlImagenArtista = reader.IsDBNull(9) ? null : reader.GetString(9),
                IdAlbum = reader.GetInt32(10),
                NombreAlbum = reader.GetString(11),
                AnioAlbum = reader.IsDBNull(12) ? null : reader.GetInt32(12),
                UrlPortadaAlbum = reader.IsDBNull(13) ? null : reader.GetString(13)
            });
        }

        return songs;
    }

    private static string NormalizePlaybackUrl(string rawUrl, int songId)
    {
        if (Uri.TryCreate(rawUrl, UriKind.Absolute, out _))
        {
            return rawUrl;
        }

        if (string.IsNullOrWhiteSpace(rawUrl))
        {
            return $"{FallbackAudioBase}SoundHelix-Song-1.mp3";
        }

        var fileName = Path.GetFileNameWithoutExtension(rawUrl).ToLowerInvariant();
        return fileName switch
        {
            "ejemplo1" => $"{FallbackAudioBase}SoundHelix-Song-1.mp3",
            "ejemplo2" => $"{FallbackAudioBase}SoundHelix-Song-2.mp3",
            "ejemplo3" => $"{FallbackAudioBase}SoundHelix-Song-3.mp3",
            "de-musica-ligera" => $"{FallbackAudioBase}SoundHelix-Song-4.mp3",
            _ => $"{FallbackAudioBase}SoundHelix-Song-{Math.Clamp(songId, 1, 16)}.mp3"
        };
    }

    private static async Task<bool> ExisteAsync(SqlConnection cn, string sql, string parameterName, int parameterValue)
    {
        await using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue(parameterName, parameterValue);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
    }

    private static async Task<bool> ExisteAsync(SqlConnection cn, string sql, params (string Name, int Value)[] parameters)
    {
        await using var cmd = new SqlCommand(sql, cn);
        foreach (var parameter in parameters)
        {
            cmd.Parameters.AddWithValue(parameter.Name, parameter.Value);
        }

        return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
    }
}
