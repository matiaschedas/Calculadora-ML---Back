using System.Text.Json;
using Matoli.Api.Models;

namespace Matoli.Api.Services;

public interface ITarifasService
{
    TarifasData GetTarifas();
}

public class TarifasService : ITarifasService
{
    private readonly TarifasData _data;

    public TarifasService(IHostEnvironment env)
    {
        var filePath = Path.Combine(env.ContentRootPath, "Data", "tarifas.json");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"No se encontró el archivo de tarifas en {filePath}");
        }

        var json = File.ReadAllText(filePath);
        try 
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            _data = JsonSerializer.Deserialize<TarifasData>(json, options)
                    ?? throw new InvalidOperationException("Error al deserializar tarifas.json");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error de JSON en: {ex.Path}");
            throw;
        }
    }

    public TarifasData GetTarifas() => _data;
}
