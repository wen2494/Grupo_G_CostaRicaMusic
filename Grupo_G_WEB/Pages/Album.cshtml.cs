using Grupo_G_WEB.Models;
using Grupo_G_WEB.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Grupo_G_WEB.Pages;

public class AlbumModel(IMusicCatalogService catalogService) : PageModel
{
    public Album? Album { get; private set; }
    public IReadOnlyList<CancionDetalleDto> Canciones { get; private set; } = [];

    public async Task OnGetAsync(int id)
    {
        Album = await catalogService.GetAlbumByIdAsync(id);
        Canciones = await catalogService.GetSongsByAlbumAsync(id);
    }
}
