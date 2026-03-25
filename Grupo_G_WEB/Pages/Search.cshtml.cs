using Grupo_G_WEB.Models;
using Grupo_G_WEB.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Grupo_G_WEB.Pages;

public class SearchModel(IMusicCatalogService catalogService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    public IReadOnlyList<CancionDetalleDto> Canciones { get; private set; } = [];
    public IReadOnlyList<Artista> Artistas { get; private set; } = [];
    public IReadOnlyList<Album> Albumes { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Canciones = await catalogService.SearchSongsAsync(Q);
        Artistas = await catalogService.SearchArtistsAsync(Q);
        Albumes = await catalogService.SearchAlbumsAsync(Q);
    }
}
