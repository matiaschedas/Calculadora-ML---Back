using Microsoft.AspNetCore.Mvc;
using Matoli.Api.Models;
using Matoli.Api.Services;

namespace Matoli.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CalculoController : ControllerBase
{
    private readonly ICalculoService _calculoService;
    private readonly ITarifasService _tarifasService;

    public CalculoController(ICalculoService calculoService, ITarifasService tarifasService)
    {
        _calculoService = calculoService;
        _tarifasService = tarifasService;
    }

    [HttpPost("calcular")]
    public IActionResult Calcular([FromBody] CalcularRequest request)
    {
        if (request == null || request.Input == null)
            return BadRequest(new { error = "Datos de entrada inválidos" });

        var tarifas = _tarifasService.GetTarifas();
        var precio = request.Precio ?? request.Input.Precio ?? 0m;
        var resultado = _calculoService.Calcular(request.Input, tarifas, precio);
        return Ok(resultado);
    }

    [HttpPost("resolver-precio")]
    public IActionResult ResolverPrecio([FromBody] ResolverPrecioRequest request)
    {
        if (request == null || request.Input == null || request.Objetivo == null)
            return BadRequest(new { error = "Solicitud inválida" });

        var tarifas = _tarifasService.GetTarifas();
        var resultado = _calculoService.ResolverPrecio(request.Input, tarifas, request.Objetivo);
        if (resultado == null)
            return NotFound(new { error = "No se pudo alcanzar el objetivo con los parámetros dados" });

        return Ok(resultado);
    }

    [HttpPost("escenarios-cuotas")]
    public IActionResult EscenariosCuotas([FromBody] EscenariosCuotasRequest request)
    {
        if (request == null || request.Input == null)
            return BadRequest(new { error = "Solicitud inválida" });

        var tarifas = _tarifasService.GetTarifas();
        var escenarios = _calculoService.EscenariosCuotas(request.Input, tarifas, request.Modo, request.Objetivo);
        return Ok(escenarios);
    }
}

public class CalcularRequest
{
    public InpModel Input { get; set; } = new();
    public decimal? Precio { get; set; }
}
