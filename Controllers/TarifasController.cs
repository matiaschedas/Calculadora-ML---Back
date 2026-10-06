using Microsoft.AspNetCore.Mvc;
using Matoli.Api.Services;

namespace Matoli.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TarifasController : ControllerBase
{
    private readonly ITarifasService _tarifasService;

    public TarifasController(ITarifasService tarifasService)
    {
        _tarifasService = tarifasService;
    }

    [HttpGet]
    public IActionResult GetTarifas()
    {
        var data = _tarifasService.GetTarifas();
        return Ok(data);
    }
}
