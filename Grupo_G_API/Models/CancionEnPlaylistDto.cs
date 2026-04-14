namespace Grupo_G_API.Models
{
    public class CancionEnPlaylistDto
    {
        public int Id { get; set; }
        public int Orden { get; set; }
        public DateTime FechaAgregado { get; set; }
        public int IdCancion { get; set; }
        public string NombreCancion { get; set; } = string.Empty;
        public int DuracionSegundos { get; set; }
        public string RutaArchivo { get; set; } = string.Empty;
        public int IdArtista { get; set; }
        public string NombreArtista { get; set; } = string.Empty;
        public int IdAlbum { get; set; }
        public string NombreAlbum { get; set; } = string.Empty;
        public string? UrlPortadaAlbum { get; set; }
        public string? UrlImagenCancion { get; set; }
    }
}
