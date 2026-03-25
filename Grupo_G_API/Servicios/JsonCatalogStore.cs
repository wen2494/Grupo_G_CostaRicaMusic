using System.Text.Json;
using Grupo_G_API.Models;

namespace Grupo_G_API.Servicios;

public class JsonCatalogStore(IWebHostEnvironment environment) : IJsonCatalogStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly string _filePath = Path.Combine(environment.ContentRootPath, "Data", "music-catalog.json");

    public async Task<CatalogDataFile> ReadAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (!File.Exists(_filePath))
            {
                var empty = new CatalogDataFile();
                await WriteInternalAsync(empty);
                return empty;
            }

            await using var stream = File.OpenRead(_filePath);
            var data = await JsonSerializer.DeserializeAsync<CatalogDataFile>(stream, JsonOptions);
            return data ?? new CatalogDataFile();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task WriteAsync(CatalogDataFile data)
    {
        await _gate.WaitAsync();
        try
        {
            await WriteInternalAsync(data);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task WriteInternalAsync(CatalogDataFile data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, data, JsonOptions);
    }
}
