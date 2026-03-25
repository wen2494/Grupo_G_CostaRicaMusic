using Grupo_G_API.Models.Api;
using Grupo_G_API.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Grupo_G_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlaylistsController(ICatalogoService catalogoService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int idUsuario = 1)
    {
        var playlists = await catalogoService.ListarPlaylistsPorUsuarioAsync(idUsuario);
        return Ok(playlists);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var playlist = await catalogoService.ObtenerPlaylistPorIdAsync(id);
        return playlist is null ? NotFound(new { mensaje = "Playlist no encontrada." }) : Ok(playlist);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlaylistRequest request)
    {
        if (request.IdUsuario <= 0 || string.IsNullOrWhiteSpace(request.Nombre))
        {
            return BadRequest(new { mensaje = "IdUsuario y Nombre son requeridos." });
        }

        var playlist = await catalogoService.CrearPlaylistAsync(request.IdUsuario, request.Nombre.Trim(), request.Descripcion?.Trim());
        return CreatedAtAction(nameof(GetById), new { id = playlist.Id }, playlist);
    }

    [HttpPost("{id:int}/canciones")]
    public async Task<IActionResult> AddSong(int id, [FromBody] AddSongToPlaylistRequest request)
    {
        if (request.IdCancion <= 0)
        {
            return BadRequest(new { mensaje = "IdCancion debe ser mayor a cero." });
        }

        var result = await catalogoService.AgregarCancionAPlaylistAsync(id, request.IdCancion);
        if (!result.Success)
        {
            return BadRequest(new { mensaje = result.Error });
        }

        var detalle = await catalogoService.ObtenerPlaylistPorIdAsync(id);
        return Ok(detalle);
    }

    [HttpDelete("{id:int}/canciones/{idCancion:int}")]
    public async Task<IActionResult> RemoveSong(int id, int idCancion)
    {
        var result = await catalogoService.QuitarCancionDePlaylistAsync(id, idCancion);
        return result.Success ? NoContent() : NotFound(new { mensaje = result.Error });
    }
}
