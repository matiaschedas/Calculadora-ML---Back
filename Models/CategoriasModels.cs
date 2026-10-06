using System.Text.Json.Serialization;

namespace Matoli.Api.Models;

public class CategoriasResumen
{
    [JsonPropertyName("min")]
    public decimal Min { get; set; }

    [JsonPropertyName("mediana")]
    public decimal Mediana { get; set; }

    [JsonPropertyName("max")]
    public decimal Max { get; set; }
}

public class CategoriasData
{
    [JsonPropertyName("generado")]
    public string Generado { get; set; } = string.Empty;

    [JsonPropertyName("exactas")]
    public int Exactas { get; set; }

    [JsonPropertyName("resumen")]
    public CategoriasResumen Resumen { get; set; } = new();

    // Each row in JSON is [id, ruta, cargoPct (null or decimal), cuotas (null or int)]
    [JsonPropertyName("filas")]
    public List<System.Text.Json.Nodes.JsonArray> Filas { get; set; } = new();
}

public class CategoriaRow
{
    public string Id { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
    public decimal? CargoPct { get; set; }
    public int? CuotasBits { get; set; }
}

public class CategoriaResultado
{
    public string Id { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
    public string LeafName { get; set; } = string.Empty;
    public string ParentPath { get; set; } = string.Empty;
    public decimal CargoPct { get; set; }
    public bool Exacto { get; set; }
    public decimal? Min { get; set; }
    public decimal? Max { get; set; }
    public int? N { get; set; }
    public int? CuotasBits { get; set; }
}
