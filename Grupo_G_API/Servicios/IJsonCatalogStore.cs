using Grupo_G_API.Models;

namespace Grupo_G_API.Servicios;

public interface IJsonCatalogStore
{
    Task<CatalogDataFile> ReadAsync();
    Task WriteAsync(CatalogDataFile data);
}
