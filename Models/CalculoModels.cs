using System.Text.Json.Serialization;

namespace Matoli.Api.Models;

public class InpModel
{
    public string Canal { get; set; } = "ml";
    public string Condicion { get; set; } = "nuevo";
    public decimal? Costo { get; set; }
    public decimal? Extras { get; set; }
    public decimal? Precio { get; set; }
    public decimal? CargoPct { get; set; }
    public string Cuotas { get; set; } = "c6";
    public string Logistica { get; set; } = "ml";
    public bool EnvioGratis { get; set; }
    public decimal? Peso { get; set; } = 0.5m;
    public decimal? Largo { get; set; }
    public decimal? Ancho { get; set; }
    public decimal? Alto { get; set; }
    public string TablaEnvio { get; set; } = "general";
    public decimal? EnvioManual { get; set; }
    public decimal? FijoManual { get; set; }
    public string FlexZona { get; set; } = "domicilio";
    public decimal? FlexCostoPropio { get; set; }
    public decimal? FlexBonifManual { get; set; }
    public decimal? EnvioPropio { get; set; }
    public decimal? EnvioMP { get; set; }
    public string MpProvincia { get; set; } = "Buenos Aires";
    public string MpPlazo { get; set; } = "10";
    public bool MpExtranjera { get; set; }
    public string Reputacion { get; set; } = "sinrep";
    public string CondFiscal { get; set; } = "cf";
    public decimal? IvaProducto { get; set; } = 21m;
    public bool IvaIncluido { get; set; } = true;
    public decimal? IibbPct { get; set; }
    public decimal? CredDebPct { get; set; }
    public decimal? AdsPct { get; set; }
    public decimal? OtrosPct { get; set; }
    public decimal? FijoMensual { get; set; }
    public decimal? UnidadesMes { get; set; }
}

public class ObjetivoModel
{
    public string Tipo { get; set; } = "margen"; // "margen" | "ganancia"
    public decimal Valor { get; set; }
    public string Redondeo { get; set; } = "entero"; // "10" | "100" | "990" | "entero"
}

public class ResolverPrecioRequest
{
    public InpModel Input { get; set; } = new();
    public ObjetivoModel Objetivo { get; set; } = new();
}

public class EscenariosCuotasRequest
{
    public InpModel Input { get; set; } = new();
    public string Modo { get; set; } = "precio"; // "precio" | "objetivo"
    public ObjetivoModel Objetivo { get; set; } = new();
}

public class CargoItemModel
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal? Pct { get; set; }
    public decimal Monto { get; set; }
    public string? Nota { get; set; }
}

public class PesoInfoModel
{
    public decimal Fisico { get; set; }
    public decimal Volumetrico { get; set; }
    public decimal Facturable { get; set; }
}

public class EnvioInfoModel
{
    public bool Obligatorio { get; set; }
    public bool Aplica { get; set; }
}

public class ResultadoModel
{
    public decimal Precio { get; set; }
    public decimal VentaNeta { get; set; }
    public decimal IvaDebito { get; set; }
    public List<CargoItemModel> Cargos { get; set; } = new();
    public decimal IvaExtra { get; set; }
    public decimal MlTotal { get; set; }
    public decimal CreditoIva { get; set; }
    public decimal MlNeto { get; set; }
    public decimal EnvioOwn { get; set; }
    public decimal Bonus { get; set; }
    public decimal Iibb { get; set; }
    public decimal CredDeb { get; set; }
    public decimal Publicidad { get; set; }
    public decimal Otros { get; set; }
    public decimal Fijos { get; set; }
    public decimal Costo { get; set; }
    public decimal Extras { get; set; }
    public decimal Ganancia { get; set; }
    public decimal Margen { get; set; }
    public decimal? Retorno { get; set; }
    public decimal Acreditado { get; set; }
    public int Dias { get; set; }
    public decimal? PrecioEquilibrio { get; set; }
    public List<string> Avisos { get; set; } = new();
    public PesoInfoModel? PesoInfo { get; set; }
    public EnvioInfoModel Envio { get; set; } = new();
    public string? FuenteEnvio { get; set; }
    public bool FijoAplica { get; set; }
}

public class CuotaOption
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal Pct { get; set; }
}

public class EscenarioCuotaModel
{
    public CuotaOption Cuota { get; set; } = new();
    public ResultadoModel? Resultado { get; set; }
}
