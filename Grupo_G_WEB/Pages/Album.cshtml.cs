using Grupo_G_WEB.Models;
using Grupo_G_WEB.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Grupo_G_WEB.Pages;

public class AlbumModel(IMusicCatalogService catalogService) : PageModel
{
    public Album? Album { get; private set; }
    public IReadOnlyList<Album> Albumes { get; private set; } = [];
    public IReadOnlyList<Artista> Artistas { get; private set; } = [];
    public IReadOnlyList<CancionDetalleDto> Canciones { get; private set; } = [];

    public async Task OnGetAsync(int? id)
    {
        if (id is null)
        {
            Albumes = await catalogService.SearchAlbumsAsync(null);
            Artistas = await catalogService.SearchArtistsAsync(null);
            return;
        }

        Album = await catalogService.GetAlbumByIdAsync(id.Value);
        Canciones = await catalogService.GetSongsByAlbumAsync(id.Value);
    }
}
