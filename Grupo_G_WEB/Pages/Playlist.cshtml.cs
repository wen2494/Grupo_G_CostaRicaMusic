using Grupo_G_WEB.Models;
using Grupo_G_WEB.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Grupo_G_WEB.Pages;

public class PlaylistModel(IMusicCatalogService catalogService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? Id { get; set; }

    [BindProperty]
    public string NombreNuevaPlaylist { get; set; } = string.Empty;

    [BindProperty]
    public string? DescripcionNuevaPlaylist { get; set; }

    [BindProperty]
    public string NombreEditarPlaylist { get; set; } = string.Empty;

    [BindProperty]
    public string? DescripcionEditarPlaylist { get; set; }

    [BindProperty]
    public int IdCancionAgregar { get; set; }

    public IReadOnlyList<Playlist> Playlists { get; private set; } = [];
    public PlaylistDetalleDto? Playlist { get; private set; }
    public IReadOnlyList<CancionDetalleDto> CatalogoCanciones { get; private set; } = [];

    [TempData]
    public string? MensajeExito { get; set; }

    [TempData]
    public string? MensajeError { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (CurrentUserId is null)
        {
            return RedirectToPage("/Login", new { returnUrl = Url.Page("/Playlist", new { id = Id }) });
        }

        await LoadAsync(Id);
        return Page();
    }

    public async Task<IActionResult> OnPostCrearAsync()
    {
        if (CurrentUserId is null)
        {
            return RedirectToPage("/Login", new { returnUrl = Url.Page("/Playlist", new { id = Id }) });
        }

        if (string.IsNullOrWhiteSpace(NombreNuevaPlaylist))
        {
            MensajeError = "Debes escribir un nombre para la playlist.";
            return RedirectToPage(new { id = Id });
        }

        var playlist = await catalogService.CreatePlaylistAsync(CurrentUserId.Value, NombreNuevaPlaylist.Trim(), DescripcionNuevaPlaylist?.Trim());
        MensajeExito = "Playlist creada correctamente.";
        return RedirectToPage(new { id = playlist.Id });
    }

    public async Task<IActionResult> OnPostEditarAsync(int id)
    {
        if (CurrentUserId is null)
        {
            return RedirectToPage("/Login", new { returnUrl = Url.Page("/Playlist", new { id }) });
        }

        if (string.IsNullOrWhiteSpace(NombreEditarPlaylist))
        {
            MensajeError = "El nombre de la playlist no puede quedar vacio.";
            return RedirectToPage(new { id });
        }

        var result = await catalogService.UpdatePlaylistAsync(id, NombreEditarPlaylist.Trim(), DescripcionEditarPlaylist?.Trim());
        if (!result.Success)
        {
            MensajeError = result.Error;
            return RedirectToPage(new { id });
        }

        MensajeExito = "Playlist actualizada.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        if (CurrentUserId is null)
        {
            return RedirectToPage("/Login", new { returnUrl = Url.Page("/Playlist") });
        }

        var result = await catalogService.DeletePlaylistAsync(id);
        if (!result.Success)
        {
            MensajeError = result.Error;
            return RedirectToPage(new { id });
        }

        var playlists = await catalogService.GetPlaylistsAsync(CurrentUserId.Value);
        var nextId = playlists.FirstOrDefault()?.Id;
        MensajeExito = "Playlist eliminada.";
        return nextId is null ? RedirectToPage() : RedirectToPage(new { id = nextId });
    }

    public async Task<IActionResult> OnPostAgregarCancionAsync(int id)
    {
        if (CurrentUserId is null)
        {
            return RedirectToPage("/Login", new { returnUrl = Url.Page("/Playlist", new { id }) });
        }

        if (IdCancionAgregar <= 0)
        {
            MensajeError = "Selecciona una cancion para agregar.";
            return RedirectToPage(new { id });
        }

        var result = await catalogService.AddSongToPlaylistAsync(id, IdCancionAgregar);
        if (!result.Success)
        {
            MensajeError = result.Error;
            return RedirectToPage(new { id });
        }

        MensajeExito = "Cancion agregada a la playlist.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostQuitarCancionAsync(int id, int idCancion)
    {
        if (CurrentUserId is null)
        {
            return RedirectToPage("/Login", new { returnUrl = Url.Page("/Playlist", new { id }) });
        }

        var result = await catalogService.RemoveSongFromPlaylistAsync(id, idCancion);
        if (!result.Success)
        {
            MensajeError = result.Error;
            return RedirectToPage(new { id });
        }

        MensajeExito = "Cancion eliminada de la playlist.";
        return RedirectToPage(new { id });
    }

    private async Task LoadAsync(int? selectedId)
    {
        Playlists = await catalogService.GetPlaylistsAsync(CurrentUserId!.Value);
        CatalogoCanciones = await catalogService.SearchSongsAsync(null);

        var currentId = selectedId ?? Playlists.FirstOrDefault()?.Id;
        if (currentId is null)
        {
            return;
        }

        Playlist = await catalogService.GetPlaylistDetalleAsync(currentId.Value);
        if (Playlist?.IdUsuario != CurrentUserId.Value)
        {
            Playlist = null;
            MensajeError = "Esta playlist pertenece a otro usuario.";
            return;
        }

        if (Playlist is not null)
        {
            NombreEditarPlaylist = Playlist.Nombre;
            DescripcionEditarPlaylist = Playlist.Descripcion;
            Id = Playlist.Id;
        }
    }

    private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");
}
