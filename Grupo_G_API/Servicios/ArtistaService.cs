using Grupo_G_API.Models;

namespace Grupo_G_API.Servicios
{
    public class ArtistaService(IJsonCatalogStore store) : IArtistaService
    {
        public async Task<List<Artista>> ListarAsync()
        {
            var data = await store.ReadAsync();
            return data.Artistas.OrderBy(item => item.Nombre).ToList();
        }

        public async Task<Artista?> ObtenerPorIdAsync(int id)
        {
            var data = await store.ReadAsync();
            return data.Artistas.FirstOrDefault(item => item.Id == id);
        }

        public async Task<List<ArtistaCancionItemDto>> ListarCancionesPorArtistaAsync(int idArtista)
        {
            var data = await store.ReadAsync();
            var artist = data.Artistas.FirstOrDefault(item => item.Id == idArtista);
            if (artist is null)
            {
                return [];
            }

            return data.Canciones
                .Where(item => item.IdArtista == idArtista)
                .OrderBy(item => item.IdAlbum)
                .ThenBy(item => item.NumeroPista)
                .Select(item =>
                {
                    var album = data.Albumes.First(record => record.Id == item.IdAlbum);
                    return new ArtistaCancionItemDto
                    {
                        Id = item.Id,
                        NombreCancion = item.Nombre,
                        DuracionSegundos = item.DuracionSegundos,
                        NumeroPista = item.NumeroPista,
                        RutaArchivo = item.RutaArchivo,
                        NombreArtista = artist.Nombre,
                        IdAlbum = album.Id,
                        NombreAlbum = album.Nombre,
                        Anio = album.Anio,
                        UrlPortadaAlbum = album.UrlPortada,
                        UrlImagenCancion = item.UrlImagen
                    };
                })
                .ToList();
        }
    }
}
