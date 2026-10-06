using System.Globalization;
using System.Text;
using System.Text.Json;
using Matoli.Api.Models;

namespace Matoli.Api.Services;

public interface ICategoriasService
{
    CategoriasData GetCategoriasData();
    List<CategoriaResultado> BuscarCategorias(string q, int limit = 40);
    CategoriaResultado? ObtenerCategoriaPorId(string id);
    (decimal pct, bool exacto, decimal? min, decimal? max, int? n) CalcularPctCategoria(string id);
}

public class CategoriasService : ICategoriasService
{
    private readonly CategoriasData _data;
    private readonly Dictionary<string, (string id, string ruta, decimal? cargoPct, int? cuotasBits)> _byId = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<(string id, string ruta, decimal? cargoPct, int? cuotasBits, string normRuta)> _filasNorm = new();
    private readonly Dictionary<string, List<decimal>> _l1 = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<decimal>> _l2 = new(StringComparer.OrdinalIgnoreCase);

    public CategoriasService(IHostEnvironment env)
    {
        var filePath = Path.Combine(env.ContentRootPath, "Data", "categorias.json");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"No se encontró el archivo categorias.json en {filePath}");
        }

        var json = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        _data = JsonSerializer.Deserialize<CategoriasData>(json, options)
                ?? throw new InvalidOperationException("Error al deserializar categorias.json");

        foreach (var arr in _data.Filas)
        {
            if (arr.Count < 2) continue;
            var id = arr[0]?.ToString() ?? string.Empty;
            var ruta = arr[1]?.ToString() ?? string.Empty;
            decimal? cargo = arr.Count > 2 && arr[2] != null && decimal.TryParse(arr[2]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var c) ? c : null;
            int? cuotas = arr.Count > 3 && arr[3] != null && int.TryParse(arr[3]?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var cu) ? cu : null;

            var item = (id, ruta, cargo, cuotas);
            _byId[id] = item;
            var norm = Normalizar(ruta);
            _filasNorm.Add((id, ruta, cargo, cuotas, norm));

            if (cargo.HasValue)
            {
                var segs = ruta.Split(" - ");
                if (segs.Length > 0)
                {
                    if (!_l1.TryGetValue(segs[0], out var list1))
                    {
                        list1 = new List<decimal>();
                        _l1[segs[0]] = list1;
                    }
                    list1.Add(cargo.Value);
                }
                if (segs.Length > 1)
                {
                    var k2 = string.Join(" - ", segs.Take(2));
                    if (!_l2.TryGetValue(k2, out var list2))
                    {
                        list2 = new List<decimal>();
                        _l2[k2] = list2;
                    }
                    list2.Add(cargo.Value);
                }
            }
        }
    }

    public CategoriasData GetCategoriasData() => _data;

    private static string Normalizar(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        var normalized = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in normalized)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }

    public List<CategoriaResultado> BuscarCategorias(string q, int limit = 40)
    {
        if (string.IsNullOrWhiteSpace(q)) return new List<CategoriaResultado>();
        var toks = Normalizar(q).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (toks.Length == 0) return new List<CategoriaResultado>();

        var resultados = new List<(int score, string id, string ruta, decimal? cargo, int? cuotas)>();

        foreach (var f in _filasNorm)
        {
            bool ok = true;
            foreach (var t in toks)
            {
                if (!f.normRuta.Contains(t))
                {
                    ok = false;
                    break;
                }
            }
            if (!ok) continue;

            var lastSep = f.normRuta.LastIndexOf(" - ");
            var leaf = lastSep == -1 ? f.normRuta : f.normRuta[(lastSep + 3)..];
            bool inLeaf = toks.All(k => leaf.Contains(k));

            int score = (inLeaf ? 0 : 1000) + leaf.Length + f.normRuta.Split(" - ").Length;
            resultados.Add((score, f.id, f.ruta, f.cargoPct, f.cuotasBits));
        }

        return resultados
            .OrderBy(r => r.score)
            .Take(limit)
            .Select(r => MapearResultado(r.id, r.ruta, r.cargo, r.cuotas))
            .ToList();
    }

    public CategoriaResultado? ObtenerCategoriaPorId(string id)
    {
        if (!_byId.TryGetValue(id, out var item)) return null;
        return MapearResultado(item.id, item.ruta, item.cargoPct, item.cuotasBits);
    }

    public (decimal pct, bool exacto, decimal? min, decimal? max, int? n) CalcularPctCategoria(string id)
    {
        if (_byId.TryGetValue(id, out var item) && item.cargoPct.HasValue)
        {
            return (item.cargoPct.Value, true, null, null, null);
        }

        if (!_byId.TryGetValue(id, out item))
        {
            return (_data.Resumen.Mediana, false, _data.Resumen.Min, _data.Resumen.Max, 0);
        }

        var segs = item.ruta.Split(" - ");
        List<decimal>? arr = null;
        if (segs.Length > 1)
        {
            var k2 = string.Join(" - ", segs.Take(2));
            _l2.TryGetValue(k2, out arr);
        }
        if ((arr == null || arr.Count == 0) && segs.Length > 0)
        {
            _l1.TryGetValue(segs[0], out arr);
        }

        if (arr == null || arr.Count == 0)
        {
            return (_data.Resumen.Mediana, false, _data.Resumen.Min, _data.Resumen.Max, 0);
        }

        var sorted = arr.OrderBy(x => x).ToList();
        var mediana = sorted[sorted.Count / 2];
        return (mediana, false, sorted[0], sorted[^1], sorted.Count);
    }

    private CategoriaResultado MapearResultado(string id, string ruta, decimal? cargo, int? cuotasBits)
    {
        var calc = CalcularPctCategoria(id);
        var lastSep = ruta.LastIndexOf(" - ");
        var leafName = lastSep == -1 ? ruta : ruta[(lastSep + 3)..];
        var parentPath = lastSep == -1 ? string.Empty : ruta[..lastSep];

        return new CategoriaResultado
        {
            Id = id,
            Ruta = ruta,
            LeafName = leafName,
            ParentPath = parentPath,
            CargoPct = calc.pct,
            Exacto = calc.exacto,
            Min = calc.min,
            Max = calc.max,
            N = calc.n,
            CuotasBits = cuotasBits
        };
    }
}
