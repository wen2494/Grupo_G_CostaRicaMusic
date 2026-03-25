using Microsoft.Data.SqlClient;
using Grupo_G_API.Models;

namespace Grupo_G_API.Servicios
{
    public class ArtistaService : IArtistaService
    {
        private readonly IConfiguration _configuration;

        public ArtistaService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string ConnectionString => _configuration.GetConnectionString("BDConnection") ?? string.Empty;

        public async Task<List<Artista>> ListarAsync()
        {
            var lista = new List<Artista>();
            await using var cn = new SqlConnection(ConnectionString);
            await cn.OpenAsync();
            const string sql = "SELECT Id, Nombre, Biografia, UrlImagen, FechaCreacion FROM Artistas ORDER BY Nombre";
            await using var cmd = new SqlCommand(sql, cn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new Artista
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Biografia = reader.IsDBNull(2) ? null : reader.GetString(2),
                    UrlImagen = reader.IsDBNull(3) ? null : reader.GetString(3),
                    FechaCreacion = reader.GetDateTime(4)
                });
            }
            return lista;
        }

        public async Task<Artista?> ObtenerPorIdAsync(int id)
        {
            await using var cn = new SqlConnection(ConnectionString);
            await cn.OpenAsync();
            await using var cmd = new SqlCommand("sp_Artista_ObtenerPorId", cn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id", id);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;
            return new Artista
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Biografia = reader.IsDBNull(2) ? null : reader.GetString(2),
                UrlImagen = reader.IsDBNull(3) ? null : reader.GetString(3),
                FechaCreacion = reader.GetDateTime(4)
            };
        }

        public async Task<List<ArtistaCancionItemDto>> ListarCancionesPorArtistaAsync(int idArtista)
        {
            var lista = new List<ArtistaCancionItemDto>();
            await using var cn = new SqlConnection(ConnectionString);
            await cn.OpenAsync();
            const string sql = """
                SELECT c.Id, c.Nombre AS NombreCancion, c.DuracionSegundos, c.NumeroPista, c.RutaArchivo,
                       a.Nombre AS NombreArtista, al.Id AS IdAlbum, al.Nombre AS NombreAlbum, al.Anio
                FROM Canciones c
                INNER JOIN Artistas a ON c.IdArtista = a.Id
                INNER JOIN Albumes al ON c.IdAlbum = al.Id
                WHERE c.IdArtista = @IdArtista
                ORDER BY al.Nombre, c.NumeroPista;
                """;
            await using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@IdArtista", idArtista);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new ArtistaCancionItemDto
                {
                    Id = reader.GetInt32(0),
                    NombreCancion = reader.GetString(1),
                    DuracionSegundos = reader.GetInt32(2),
                    NumeroPista = reader.GetInt32(3),
                    RutaArchivo = reader.GetString(4),
                    NombreArtista = reader.GetString(5),
                    IdAlbum = reader.GetInt32(6),
                    NombreAlbum = reader.GetString(7),
                    Anio = reader.IsDBNull(8) ? null : reader.GetInt32(8)
                });
            }
            return lista;
        }
    }
}
