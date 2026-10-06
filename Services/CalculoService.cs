using Matoli.Api.Models;

namespace Matoli.Api.Services;

public interface ICalculoService
{
    ResultadoModel Calcular(InpModel inp, TarifasData D, decimal precio);
    ResultadoModel? ResolverPrecio(InpModel inp, TarifasData D, ObjetivoModel objetivo);
    List<EscenarioCuotaModel> EscenariosCuotas(InpModel inp, TarifasData D, string modo, ObjetivoModel objetivo);
    decimal Redondear(decimal p, string modo);
}

public class CalculoService : ICalculoService
{
    private const decimal Eps = 1e-6m;

    private static decimal Num(decimal? v, decimal d = 0m) => v ?? d;
    private static decimal Pct(decimal v) => v / 100m;
    private static decimal Sum(IEnumerable<decimal> arr) => arr.Sum();

    private static int TramoPeso(decimal kg, decimal?[] pesos)
    {
        for (var i = 0; i < pesos.Length; i++)
        {
            if (pesos[i] == null || kg <= pesos[i]) return i;
        }
        return pesos.Length - 1;
    }

    private static PesoInfoModel PesoFacturable(InpModel inp, TarifasData D)
    {
        var fisico = Num(inp.Peso);
        var vol = (Num(inp.Largo) * Num(inp.Ancho) * Num(inp.Alto)) / D.Meta.DivisorVolumetrico;
        return new PesoInfoModel
        {
            Fisico = fisico,
            Volumetrico = vol,
            Facturable = Math.Max(fisico, vol)
        };
    }

    private static int ColumnaCostoFijo(decimal precio, TarifasData D)
    {
        var t = D.CostoFijo.TramosPrecio;
        return precio < t[0] ? 0 : precio < t[1] ? 1 : 2;
    }

    private static bool UsaTablaPeso(string logistica) => logistica == "ml" || logistica == "full";

    private static decimal CostoFijo(InpModel inp, decimal precio, TarifasData D, decimal kg)
    {
        if (precio <= 0 || precio >= D.Meta.UmbralEnvioGratis) return 0m;
        if (inp.FijoManual.HasValue) return Math.Max(0m, inp.FijoManual.Value);

        var col = ColumnaCostoFijo(precio, D);
        if (UsaTablaPeso(inp.Logistica))
        {
            var tramo = TramoPeso(kg, D.Pesos);
            var fila = D.CostoFijo.PorPeso[tramo];
            return fila[1 + col] ?? 0m;
        }
        return D.CostoFijo.PorPrecio[col];
    }

    private static EnvioInfoModel EnvioGratisAplica(InpModel inp, decimal precio, TarifasData D)
    {
        var obligatorio = inp.Condicion != "usado" && precio >= D.Meta.UmbralEnvioGratis;
        return new EnvioInfoModel
        {
            Obligatorio = obligatorio,
            Aplica = obligatorio || inp.EnvioGratis
        };
    }

    private static (decimal monto, string tabla) CostoEnvioTabla(InpModel inp, decimal precio, TarifasData D, decimal kg)
    {
        var i = TramoPeso(kg, D.Pesos);
        var umbral = D.Meta.UmbralEnvioGratis;
        var rep = inp.Reputacion == "roja" ? "roja" : inp.Reputacion == "amarilla" ? "amarilla" : "verde";

        if (inp.Condicion == "usado") return (D.Envio.Usados[i][1] ?? 0m, "usados");

        if (precio >= umbral && rep == "verde" && inp.TablaEnvio == "zapatillas")
        {
            var f = D.Envio.Zapatillas[i];
            var montoZ = precio < 50000 ? (f[1] ?? 0m) : precio < 90000 ? (f[2] ?? 0m) : (f[3] ?? 0m);
            return (montoZ, "zapatillas");
        }

        var fila = rep switch
        {
            "verde" => D.Envio.Verde[i],
            "amarilla" => D.Envio.Amarilla[i],
            "roja" => D.Envio.Roja[i],
            _ => D.Envio.Verde[i]
        };

        if (rep == "roja")
        {
            return (precio < umbral ? (fila[1] ?? 0m) : (fila[2] ?? 0m), "roja");
        }

        var monto = precio < umbral ? (fila[1] ?? 0m) : precio < 50000 ? (fila[2] ?? 0m) : (fila[3] ?? 0m);
        return (monto, rep);
    }

    private static decimal BonificacionFlex(InpModel inp, decimal precio, TarifasData D)
    {
        if (inp.FlexBonifManual.HasValue) return Math.Max(0m, inp.FlexBonifManual.Value);
        var z = D.Flex.Zonas.FirstOrDefault(x => x.Id == inp.FlexZona) ?? D.Flex.Zonas[0];
        if (precio < D.Meta.UmbralEnvioGratis) return z.Menor33;
        return inp.Reputacion == "verde" && inp.Condicion != "usado" ? z.Desde33verde : 0m;
    }

    private static decimal CuotaPct(InpModel inp, TarifasData D)
    {
        var cuota = D.Cuotas.FirstOrDefault(x => x.Id == inp.Cuotas);
        return cuota?.Pct ?? 0m;
    }

    private static int DiasLiberacionML(InpModel inp, TarifasData D)
    {
        if (inp.Logistica == "full") return D.Liberacion.Full;
        if (inp.Logistica == "propio") return D.Liberacion.Propio;
        if (inp.Reputacion == "sinrep") return D.Liberacion.SinReputacion;
        return inp.Condicion == "usado" ? D.Liberacion.ConReputacionUsado : D.Liberacion.ConReputacionNuevo;
    }

    private static BaseCalculoResult BaseML(InpModel inp, decimal precio, TarifasData D)
    {
        var P = precio;
        var avisos = new List<string>();
        var pesoInfo = PesoFacturable(inp, D);
        var kg = pesoInfo.Facturable;
        var envio = EnvioGratisAplica(inp, P, D);

        var cargos = new List<CargoItemModel>
        {
            new() { Id = "cargoVender", Label = "Cargo por vender", Pct = inp.CargoPct, Monto = P * Pct(Num(inp.CargoPct)) }
        };

        var cp = CuotaPct(inp, D);
        if (cp > 0)
        {
            cargos.Add(new CargoItemModel { Id = "cuotas", Label = "Costo por ofrecer cuotas", Pct = cp, Monto = P * cp / 100m });
        }

        var fijo = CostoFijo(inp, P, D, kg);
        if (fijo > 0)
        {
            cargos.Add(new CargoItemModel { Id = "fijo", Label = "Costo fijo por unidad vendida", Monto = fijo });
        }

        decimal envioOwn = 0m;
        decimal bonus = 0m;
        string? fuenteEnvio = null;

        if (UsaTablaPeso(inp.Logistica))
        {
            if (envio.Aplica)
            {
                var manual = inp.EnvioManual.HasValue && inp.EnvioManual.Value > 0;
                var t = manual ? (monto: inp.EnvioManual!.Value, tabla: "manual") : CostoEnvioTabla(inp, P, D, kg);
                cargos.Add(new CargoItemModel { Id = "envio", Label = "Envío gratis a tu cargo", Monto = t.monto });
                fuenteEnvio = t.tabla;
            }
        }
        else if (inp.Logistica == "flex")
        {
            envioOwn = Num(inp.FlexCostoPropio);
            bonus = BonificacionFlex(inp, P, D);
        }
        else
        {
            envioOwn = envio.Aplica ? Num(inp.EnvioPropio) : 0m;
        }

        if (P > 0 && P < D.Meta.PrecioMinimo)
        {
            avisos.Add($"El precio mínimo de publicación en Mercado Libre es $ {D.Meta.PrecioMinimo:N0}.");
        }
        if (envio.Obligatorio && inp.Logistica == "propio")
        {
            avisos.Add($"Desde $ {D.Meta.UmbralEnvioGratis:N0} los productos nuevos deben ofrecer envío gratis con Mercado Envíos.");
        }
        if (UsaTablaPeso(inp.Logistica) && kg <= 0 && envio.Aplica)
        {
            avisos.Add("Cargá el peso del paquete para calcular el envío y el costo fijo con precisión.");
        }

        return new BaseCalculoResult
        {
            P = P,
            Cargos = cargos,
            IvaIncluido = inp.IvaIncluido,
            EnvioOwn = envioOwn,
            Bonus = bonus,
            Dias = DiasLiberacionML(inp, D),
            Avisos = avisos,
            PesoInfo = pesoInfo,
            Envio = envio,
            FuenteEnvio = fuenteEnvio,
            FijoAplica = fijo > 0
        };
    }

    private static MpGrupoModel GrupoMp(string provincia, TarifasData D)
    {
        return D.Mp.Grupos.FirstOrDefault(g => g.Provincias.Contains(provincia)) ?? D.Mp.Grupos[0];
    }

    private static BaseCalculoResult BaseMp(InpModel inp, decimal precio, TarifasData D)
    {
        var g = GrupoMp(inp.MpProvincia, D);
        var idx = 0;
        for (var i = 0; i < D.Mp.Plazos.Count; i++)
        {
            if (D.Mp.Plazos[i].Id == inp.MpPlazo) idx = i;
        }
        var p = g.Pct[idx];
        var cargos = new List<CargoItemModel>
        {
            new() { Id = "mp", Label = "Costo de Mercado Pago", Pct = p, Nota = "+ IVA", Monto = precio * p / 100m }
        };
        var avisos = new List<string>();
        if (inp.MpExtranjera)
        {
            cargos.Add(new CargoItemModel { Id = "mpExt", Label = "Recargo tarjeta extranjera / Sucrédito (3% del costo)", Monto = precio * p / 100m * 0.03m });
        }

        return new BaseCalculoResult
        {
            P = precio,
            Cargos = cargos,
            IvaIncluido = false,
            EnvioOwn = Num(inp.EnvioMP),
            Bonus = 0m,
            Dias = D.Mp.Plazos[idx].Dias,
            Avisos = avisos,
            PesoInfo = null,
            Envio = new EnvioInfoModel { Obligatorio = false, Aplica = false },
            FuenteEnvio = null,
            FijoAplica = false
        };
    }

    private static ResultadoModel Liquidar(InpModel inp, BaseCalculoResult baseRes, TarifasData D)
    {
        var P = baseRes.P;
        var ri = inp.CondFiscal == "ri";
        var ivaProd = ri ? Num(inp.IvaProducto, 21m) : 0m;
        var ventaNeta = ri ? P / (1m + ivaProd / 100m) : P;
        var ivaDebito = P - ventaNeta;
        var ivaRate = D.Meta.IvaGeneral / 100m;

        var subtotal = Sum(baseRes.Cargos.Select(c => c.Monto));
        var ivaExtra = baseRes.IvaIncluido ? 0m : subtotal * ivaRate;
        var mlTotal = subtotal + ivaExtra;
        var creditoIva = ri ? mlTotal * (ivaRate / (1m + ivaRate)) : 0m;
        var mlNeto = mlTotal - creditoIva;

        var iibb = ventaNeta * Pct(Num(inp.IibbPct));
        var credDeb = Math.Max(0m, P - mlTotal + baseRes.Bonus) * Pct(Num(inp.CredDebPct));
        var publicidad = ventaNeta * Pct(Num(inp.AdsPct));
        var otros = ventaNeta * Pct(Num(inp.OtrosPct));
        var fijos = Num(inp.UnidadesMes) > 0 ? Num(inp.FijoMensual) / Num(inp.UnidadesMes) : 0m;
        var costo = Num(inp.Costo);
        var extras = Num(inp.Extras);

        var ganancia = ventaNeta - mlNeto - baseRes.EnvioOwn + baseRes.Bonus - iibb - credDeb - publicidad - otros - fijos - costo - extras;
        var inversion = costo + extras;

        return new ResultadoModel
        {
            Precio = P,
            VentaNeta = ventaNeta,
            IvaDebito = ivaDebito,
            Cargos = baseRes.Cargos,
            IvaExtra = ivaExtra,
            MlTotal = mlTotal,
            CreditoIva = creditoIva,
            MlNeto = mlNeto,
            EnvioOwn = baseRes.EnvioOwn,
            Bonus = baseRes.Bonus,
            Iibb = iibb,
            CredDeb = credDeb,
            Publicidad = publicidad,
            Otros = otros,
            Fijos = fijos,
            Costo = costo,
            Extras = extras,
            Ganancia = ganancia,
            Margen = ventaNeta > 0 ? ganancia / ventaNeta : 0m,
            Retorno = inversion > 0 ? ganancia / inversion : null,
            Acreditado = P - mlTotal + baseRes.Bonus - iibb - credDeb,
            Dias = baseRes.Dias,
            Avisos = baseRes.Avisos,
            PesoInfo = baseRes.PesoInfo,
            Envio = baseRes.Envio,
            FuenteEnvio = baseRes.FuenteEnvio,
            FijoAplica = baseRes.FijoAplica
        };
    }

    public ResultadoModel Calcular(InpModel inp, TarifasData D, decimal precio)
    {
        var P = Math.Max(0m, precio);
        var baseRes = inp.Canal == "mp" ? BaseMp(inp, P, D) : BaseML(inp, P, D);
        return Liquidar(inp, baseRes, D);
    }

    public decimal Redondear(decimal p, string modo)
    {
        return modo switch
        {
            "10" => Math.Ceiling(p / 10m - 1e-9m) * 10m,
            "100" => Math.Ceiling(p / 100m - 1e-9m) * 100m,
            "990" => Math.Max(0m, Math.Ceiling((p - 990m) / 1000m - 1e-9m)) * 1000m + 990m,
            _ => Math.Ceiling(p - 1e-9m)
        };
    }

    private static decimal[] Fronteras(TarifasData D)
    {
        var t = D.CostoFijo.TramosPrecio;
        return new[] { 0m, t[0], t[1], D.Meta.UmbralEnvioGratis, 50000m, 90000m, decimal.MaxValue };
    }

    public ResultadoModel? ResolverPrecio(InpModel inp, TarifasData D, ObjetivoModel objetivo)
    {
        var tipo = objetivo.Tipo == "ganancia" ? "ganancia" : "margen";
        var valor = objetivo.Valor;
        var minimo = inp.Canal == "mp" ? 1m : D.Meta.PrecioMinimo;

        decimal F(decimal P)
        {
            var r = Calcular(inp, D, P);
            return r.Ganancia - (tipo == "margen" ? (valor / 100m) * r.VentaNeta : valor);
        }

        var fr = Fronteras(D);
        for (var b = 0; b < fr.Length - 1; b++)
        {
            var lo = fr[b];
            var hi = fr[b + 1];
            if (hi <= minimo) continue;
            var p1 = Math.Max(lo, minimo);
            var p2 = hi != decimal.MaxValue ? Math.Min(p1 + 1000m, (p1 + hi) / 2m) : p1 + 1000m;
            var f1 = F(p1);
            var f2 = F(p2);
            var alfa = (f2 - f1) / (p2 - p1);

            decimal cand;
            if (alfa > Eps) cand = Math.Max(p1 - f1 / alfa, p1);
            else if (f1 >= -Eps) cand = p1;
            else continue;

            cand = Redondear(cand, objetivo.Redondeo);
            if (cand >= hi) continue;
            if (F(cand) < -1e-4m) continue;
            return Calcular(inp, D, cand);
        }
        return null;
    }

    public List<EscenarioCuotaModel> EscenariosCuotas(InpModel inp, TarifasData D, string modo, ObjetivoModel objetivo)
    {
        return D.Cuotas.Select(c =>
        {
            var i2 = new InpModel
            {
                Canal = inp.Canal, Condicion = inp.Condicion, Costo = inp.Costo, Extras = inp.Extras, Precio = inp.Precio,
                CargoPct = inp.CargoPct, Cuotas = c.Id, Logistica = inp.Logistica, EnvioGratis = inp.EnvioGratis,
                Peso = inp.Peso, Largo = inp.Largo, Ancho = inp.Ancho, Alto = inp.Alto, TablaEnvio = inp.TablaEnvio,
                EnvioManual = inp.EnvioManual, FijoManual = inp.FijoManual, FlexZona = inp.FlexZona, FlexCostoPropio = inp.FlexCostoPropio,
                FlexBonifManual = inp.FlexBonifManual, EnvioPropio = inp.EnvioPropio, EnvioMP = inp.EnvioMP, MpProvincia = inp.MpProvincia,
                MpPlazo = inp.MpPlazo, MpExtranjera = inp.MpExtranjera, Reputacion = inp.Reputacion, CondFiscal = inp.CondFiscal,
                IvaProducto = inp.IvaProducto, IvaIncluido = inp.IvaIncluido, IibbPct = inp.IibbPct, CredDebPct = inp.CredDebPct,
                AdsPct = inp.AdsPct, OtrosPct = inp.OtrosPct, FijoMensual = inp.FijoMensual, UnidadesMes = inp.UnidadesMes
            };
            var r = modo == "precio" ? Calcular(i2, D, inp.Precio ?? 0m) : ResolverPrecio(i2, D, objetivo);
            return new EscenarioCuotaModel { Cuota = c, Resultado = r };
        }).ToList();
    }

    private class BaseCalculoResult
    {
        public decimal P { get; set; }
        public List<CargoItemModel> Cargos { get; set; } = new();
        public bool IvaIncluido { get; set; }
        public decimal EnvioOwn { get; set; }
        public decimal Bonus { get; set; }
        public int Dias { get; set; }
        public List<string> Avisos { get; set; } = new();
        public PesoInfoModel? PesoInfo { get; set; }
        public EnvioInfoModel Envio { get; set; } = new();
        public string? FuenteEnvio { get; set; }
        public bool FijoAplica { get; set; }
    }
}
