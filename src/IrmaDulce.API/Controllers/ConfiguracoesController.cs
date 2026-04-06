using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IrmaDulce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master,Administrativo")]
public class ConfiguracoesController : ControllerBase
{
    private readonly IConfiguracaoService _configuracaoService;

    public ConfiguracoesController(IConfiguracaoService configuracaoService)
    {
        _configuracaoService = configuracaoService;
    }

    [HttpGet]
    public async Task<ActionResult<ConfiguracaoResponse>> Get()
    {
        var result = await _configuracaoService.GetAsync();
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<ConfiguracaoResponse>> Atualizar(ConfiguracaoRequest request)
    {
        var result = await _configuracaoService.AtualizarAsync(request);
        return Ok(result);
    }
}
