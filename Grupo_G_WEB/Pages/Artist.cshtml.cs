using Grupo_G_WEB.Models;
using Grupo_G_WEB.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Grupo_G_WEB.Pages;

public class ArtistModel(IMusicCatalogService catalogService) : PageModel
{
    public Artista? Artista { get; private set; }
    public IReadOnlyList<CancionDetalleDto> Canciones { get; private set; } = [];

    public async Task OnGetAsync(int id)
    {
        Artista = await catalogService.GetArtistByIdAsync(id);
        Canciones = await catalogService.GetSongsByArtistAsync(id);
    }
}
