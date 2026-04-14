namespace Grupo_G_WEB.Models
{
    public class CancionDetalleDto
    {
        public int Id { get; set; }
        public string NombreCancion { get; set; } = string.Empty;
        public int DuracionSegundos { get; set; }
        public int NumeroPista { get; set; }
        public string RutaArchivo { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public int IdArtista { get; set; }
        public string NombreArtista { get; set; } = string.Empty;
        public string? BiografiaArtista { get; set; }
        public string? UrlImagenArtista { get; set; }
        public int IdAlbum { get; set; }
        public string NombreAlbum { get; set; } = string.Empty;
        public int? AnioAlbum { get; set; }
        public string? UrlPortadaAlbum { get; set; }
        public string? UrlImagenCancion { get; set; }
    }
}
