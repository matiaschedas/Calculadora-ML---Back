using System.Text.Json.Serialization;

namespace Matoli.Api.Models;

public class MetaModel
{
    [JsonPropertyName("actualizado")]
    public string Actualizado { get; set; } = string.Empty;

    [JsonPropertyName("umbralEnvioGratis")]
    public decimal UmbralEnvioGratis { get; set; }

    [JsonPropertyName("precioMinimo")]
    public decimal PrecioMinimo { get; set; }

    [JsonPropertyName("precioMinimoMasivo")]
    public decimal PrecioMinimoMasivo { get; set; }

    [JsonPropertyName("divisorVolumetrico")]
    public decimal DivisorVolumetrico { get; set; }

    [JsonPropertyName("ivaGeneral")]
    public decimal IvaGeneral { get; set; }

    [JsonPropertyName("credDebPct")]
    public decimal CredDebPct { get; set; }
}

public class CargoPorVenderModel
{
    [JsonPropertyName("minPct")]
    public decimal MinPct { get; set; }

    [JsonPropertyName("maxPct")]
    public decimal MaxPct { get; set; }

    [JsonPropertyName("defaultPct")]
    public decimal DefaultPct { get; set; }

    [JsonPropertyName("nota")]
    public string Nota { get; set; } = string.Empty;
}

public class CostoFijoModel
{
    [JsonPropertyName("tramosPrecio")]
    public decimal[] TramosPrecio { get; set; } = Array.Empty<decimal>();

    [JsonPropertyName("porPrecio")]
    public decimal[] PorPrecio { get; set; } = Array.Empty<decimal>();

    [JsonPropertyName("porPeso")]
    public decimal?[][] PorPeso { get; set; } = Array.Empty<decimal?[]>();
}

public class EnvioModel
{
    [JsonPropertyName("verde")]
    public decimal?[][] Verde { get; set; } = Array.Empty<decimal?[]>();

    [JsonPropertyName("amarilla")]
    public decimal?[][] Amarilla { get; set; } = Array.Empty<decimal?[]>();

    [JsonPropertyName("roja")]
    public decimal?[][] Roja { get; set; } = Array.Empty<decimal?[]>();

    [JsonPropertyName("usados")]
    public decimal?[][] Usados { get; set; } = Array.Empty<decimal?[]>();

    [JsonPropertyName("zapatillas")]
    public decimal?[][] Zapatillas { get; set; } = Array.Empty<decimal?[]>();
}

public class FlexZonaModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("menor33")]
    public decimal Menor33 { get; set; }

    [JsonPropertyName("desde33verde")]
    public decimal Desde33verde { get; set; }
}

public class FlexModel
{
    [JsonPropertyName("fuente")]
    public string Fuente { get; set; } = string.Empty;

    [JsonPropertyName("zonas")]
    public List<FlexZonaModel> Zonas { get; set; } = new();

    [JsonPropertyName("nota")]
    public string Nota { get; set; } = string.Empty;
}

public class FullItemModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("detalle")]
    public string Detalle { get; set; } = string.Empty;

    [JsonPropertyName("tarifa")]
    public decimal Tarifa { get; set; }
}

public class FullModel
{
    [JsonPropertyName("almacenamientoDiario")]
    public List<FullItemModel> AlmacenamientoDiario { get; set; } = new();

    [JsonPropertyName("stockAntiguoMeses")]
    public int StockAntiguoMeses { get; set; }

    [JsonPropertyName("stockAntiguoMesesSuper")]
    public int StockAntiguoMesesSuper { get; set; }

    [JsonPropertyName("notas")]
    public List<string> Notas { get; set; } = new();
}

public class LiberacionModel
{
    [JsonPropertyName("conReputacionNuevo")]
    public int ConReputacionNuevo { get; set; }

    [JsonPropertyName("conReputacionUsado")]
    public int ConReputacionUsado { get; set; }

    [JsonPropertyName("sinReputacion")]
    public int SinReputacion { get; set; }

    [JsonPropertyName("full")]
    public int Full { get; set; }

    [JsonPropertyName("propio")]
    public int Propio { get; set; }
}

public class MpPlazoModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("dias")]
    public int Dias { get; set; }
}

public class MpGrupoModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("pct")]
    public decimal[] Pct { get; set; } = Array.Empty<decimal>();

    [JsonPropertyName("provincias")]
    public List<string> Provincias { get; set; } = new();
}

public class MpModel
{
    [JsonPropertyName("vigencia")]
    public string Vigencia { get; set; } = string.Empty;

    [JsonPropertyName("plazos")]
    public List<MpPlazoModel> Plazos { get; set; } = new();

    [JsonPropertyName("grupos")]
    public List<MpGrupoModel> Grupos { get; set; } = new();

    [JsonPropertyName("nota")]
    public string Nota { get; set; } = string.Empty;
}

public class ImpuestoSection
{
    [JsonPropertyName("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<string> Items { get; set; } = new();
}

public class CambioItem
{
    [JsonPropertyName("fecha")]
    public string Fecha { get; set; } = string.Empty;

    [JsonPropertyName("texto")]
    public string Texto { get; set; } = string.Empty;

    [JsonPropertyName("link")]
    public string? Link { get; set; }
}

public class FuenteItem
{
    [JsonPropertyName("t")]
    public string Titulo { get; set; } = string.Empty;
    [JsonPropertyName("u")]
    public string Url { get; set; } = string.Empty;
}

public class FuentesModel
{
    [JsonPropertyName("oficiales")]
    public List<FuenteItem> Oficiales { get; set; } = new();

    [JsonPropertyName("terceros")]
    public List<FuenteItem> Terceros { get; set; } = new();
}

public class TarifasData
{
    [JsonPropertyName("meta")]
    public MetaModel Meta { get; set; } = new();

    [JsonPropertyName("cargoPorVender")]
    public CargoPorVenderModel CargoPorVender { get; set; } = new();

    [JsonPropertyName("cuotas")]
    public List<CuotaOption> Cuotas { get; set; } = new();

    [JsonPropertyName("pesos")]
    public decimal?[] Pesos { get; set; } = Array.Empty<decimal?>();

    [JsonPropertyName("costoFijo")]
    public CostoFijoModel CostoFijo { get; set; } = new();

    [JsonPropertyName("envio")]
    public EnvioModel Envio { get; set; } = new();

    [JsonPropertyName("flex")]
    public FlexModel Flex { get; set; } = new();

    [JsonPropertyName("full")]
    public FullModel Full { get; set; } = new();

    [JsonPropertyName("liberacion")]
    public LiberacionModel Liberacion { get; set; } = new();

    [JsonPropertyName("mp")]
    public MpModel Mp { get; set; } = new();

    [JsonPropertyName("impuestos")]
    public List<ImpuestoSection> Impuestos { get; set; } = new();

    [JsonPropertyName("cambios")]
    public List<CambioItem> Cambios { get; set; } = new();

    [JsonPropertyName("fuentes")]
    public FuentesModel Fuentes { get; set; } = new();
}
