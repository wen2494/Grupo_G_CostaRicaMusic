namespace Grupo_G_API.Models
{
    /// <summary>Resultado de sp_Artista_Canciones_Listar: canción con datos de álbum.</summary>
    public class ArtistaCancionItemDto
    {
        public int Id { get; set; }
        public string NombreCancion { get; set; } = string.Empty;
        public int DuracionSegundos { get; set; }
        public int NumeroPista { get; set; }
        public string RutaArchivo { get; set; } = string.Empty;
        public string NombreArtista { get; set; } = string.Empty;
        public int IdAlbum { get; set; }
        public string NombreAlbum { get; set; } = string.Empty;
        public int? Anio { get; set; }
        public string? UrlPortadaAlbum { get; set; }
        public string? UrlImagenCancion { get; set; }
    }
}
