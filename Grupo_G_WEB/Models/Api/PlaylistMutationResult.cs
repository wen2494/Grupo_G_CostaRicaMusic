using Grupo_G_WEB.Models;

namespace Grupo_G_WEB.Models.Api;

public sealed record PlaylistMutationResult(bool Success, string? Error = null, PlaylistDetalleDto? Playlist = null);
