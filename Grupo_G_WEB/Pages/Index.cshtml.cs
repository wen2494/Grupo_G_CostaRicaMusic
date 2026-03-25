using Grupo_G_WEB.Models;
using Grupo_G_WEB.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Grupo_G_WEB.Pages;

public class IndexModel(IMusicCatalogService catalogService) : PageModel
{
    public IReadOnlyList<CancionDetalleDto> Canciones { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Canciones = (await catalogService.SearchSongsAsync(null)).Take(12).ToList();
    }
}
