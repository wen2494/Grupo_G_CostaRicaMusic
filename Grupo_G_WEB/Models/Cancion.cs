namespace Grupo_G_WEB.Models
{
    public class Cancion
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int IdAlbum { get; set; }
        public int IdArtista { get; set; }
        public int DuracionSegundos { get; set; }
        public int NumeroPista { get; set; } = 1;
        public string RutaArchivo { get; set; } = string.Empty;
        public string? UrlImagen { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
