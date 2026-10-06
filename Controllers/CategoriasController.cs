using Microsoft.AspNetCore.Mvc;
using Matoli.Api.Services;

namespace Matoli.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriasService _categoriasService;

    public CategoriasController(ICategoriasService categoriasService)
    {
        _categoriasService = categoriasService;
    }

    [HttpGet]
    public IActionResult GetMeta()
    {
        var data = _categoriasService.GetCategoriasData();
        return Ok(new
        {
            generado = data.Generado,
            exactas = data.Exactas,
            resumen = data.Resumen
        });
    }

    [HttpGet("buscar")]
    public IActionResult Buscar([FromQuery] string q, [FromQuery] int limit = 40)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(Array.Empty<object>());
        }
        var resultados = _categoriasService.BuscarCategorias(q, limit);
        return Ok(resultados);
    }

    [HttpGet("{id}")]
    public IActionResult ObtenerPorId(string id)
    {
        var cat = _categoriasService.ObtenerCategoriaPorId(id);
        if (cat == null) return NotFound(new { error = "Categoría no encontrada" });
        return Ok(cat);
    }
}
