using Grupo_G_API.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Grupo_G_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CatalogoController(ICatalogoService catalogoService) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] string? tipo = "todo")
    {
        var kind = (tipo ?? "todo").Trim().ToLowerInvariant();
        var canciones = kind is "todo" or "canciones"
            ? await catalogoService.BuscarCancionesAsync(q)
            : [];
        var albumes = kind is "todo" or "albumes"
            ? await catalogoService.BuscarAlbumesAsync(q)
            : [];
        var artistas = kind is "todo" or "artistas"
            ? await catalogoService.BuscarArtistasAsync(q)
            : [];

        return Ok(new
        {
            query = q,
            canciones,
            albumes,
            artistas
        });
    }

    [HttpGet("canciones/{id:int}")]
    public async Task<IActionResult> GetSongById(int id)
    {
        var song = await catalogoService.ObtenerCancionPorIdAsync(id);
        return song is null ? NotFound(new { mensaje = "Cancion no encontrada." }) : Ok(song);
    }

    [HttpGet("canciones/{id:int}/reproduccion")]
    public async Task<IActionResult> GetPlaybackById(int id)
    {
        var song = await catalogoService.ObtenerCancionPorIdAsync(id);
        if (song is null)
        {
            return NotFound(new { mensaje = "Cancion no encontrada." });
        }

        return Ok(new
        {
            id = song.Id,
            nombre = song.NombreCancion,
            urlReproduccion = song.RutaArchivo,
            duracionSegundos = song.DuracionSegundos
        });
    }

    [HttpGet("albumes/{id:int}")]
    public async Task<IActionResult> GetAlbumById(int id)
    {
        var album = await catalogoService.ObtenerAlbumPorIdAsync(id);
        return album is null ? NotFound(new { mensaje = "Album no encontrado." }) : Ok(album);
    }

    [HttpGet("albumes/{id:int}/canciones")]
    public async Task<IActionResult> GetAlbumSongs(int id)
    {
        var songs = await catalogoService.ListarCancionesPorAlbumAsync(id);
        return Ok(songs);
    }
}
