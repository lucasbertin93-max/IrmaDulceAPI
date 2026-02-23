using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IrmaDulce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master,Administrativo")]
public class CursosController : ControllerBase
{
    private readonly ICursoService _cursoService;

    public CursosController(ICursoService cursoService)
    {
        _cursoService = cursoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CursoResponse>>> GetAll()
    {
        var cursos = await _cursoService.GetAllAsync();
        return Ok(cursos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CursoResponse>> GetById(int id)
    {
        var curso = await _cursoService.GetByIdAsync(id);
        return curso == null ? NotFound() : Ok(curso);
    }

    [HttpPost]
    public async Task<ActionResult<CursoResponse>> Criar([FromBody] CursoRequest request)
    {
        var curso = await _cursoService.CriarAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = curso.Id }, curso);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CursoResponse>> Atualizar(int id, [FromBody] CursoRequest request)
    {
        try
        {
            var curso = await _cursoService.AtualizarAsync(id, request);
            return Ok(curso);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id)
    {
        try
        {
            await _cursoService.DeletarAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{cursoId}/disciplinas/{disciplinaId}")]
    public async Task<IActionResult> VincularDisciplina(int cursoId, int disciplinaId, [FromQuery] int? semestre)
    {
        try
        {
            await _cursoService.VincularDisciplinaAsync(cursoId, disciplinaId, semestre);
            return Ok(new { message = "Disciplina vinculada com sucesso." });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpDelete("{cursoId}/disciplinas/{disciplinaId}")]
    public async Task<IActionResult> DesvincularDisciplina(int cursoId, int disciplinaId)
    {
        try
        {
            await _cursoService.DesvincularDisciplinaAsync(cursoId, disciplinaId);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master,Administrativo")]
public class DisciplinasController : ControllerBase
{
    private readonly IDisciplinaService _disciplinaService;

    public DisciplinasController(IDisciplinaService disciplinaService)
    {
        _disciplinaService = disciplinaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DisciplinaResponse>>> GetAll()
    {
        var disciplinas = await _disciplinaService.GetAllAsync();
        return Ok(disciplinas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DisciplinaResponse>> GetById(int id)
    {
        var d = await _disciplinaService.GetByIdAsync(id);
        return d == null ? NotFound() : Ok(d);
    }

    [HttpPost]
    public async Task<ActionResult<DisciplinaResponse>> Criar([FromBody] DisciplinaRequest request)
    {
        var d = await _disciplinaService.CriarAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = d.Id }, d);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DisciplinaResponse>> Atualizar(int id, [FromBody] DisciplinaRequest request)
    {
        try
        {
            var d = await _disciplinaService.AtualizarAsync(id, request);
            return Ok(d);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id)
    {
        try
        {
            await _disciplinaService.DeletarAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
