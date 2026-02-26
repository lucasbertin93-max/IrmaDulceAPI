using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IrmaDulce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master,Administrativo")]
public class TurmasController : ControllerBase
{
    private readonly ITurmaService _turmaService;

    public TurmasController(ITurmaService turmaService)
    {
        _turmaService = turmaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TurmaResponse>>> GetAll()
    {
        var turmas = await _turmaService.GetAllAsync();
        return Ok(turmas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TurmaResponse>> GetById(int id)
    {
        var turma = await _turmaService.GetByIdAsync(id);
        return turma == null ? NotFound() : Ok(turma);
    }

    [HttpGet("pesquisar")]
    public async Task<ActionResult<IEnumerable<TurmaResponse>>> Pesquisar([FromQuery] string termo)
    {
        var turmas = await _turmaService.PesquisarAsync(termo);
        return Ok(turmas);
    }

    [HttpPost]
    public async Task<ActionResult<TurmaResponse>> Criar([FromBody] TurmaCreateRequest request)
    {
        try
        {
            var turma = await _turmaService.CriarAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = turma.Id }, turma);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TurmaResponse>> Atualizar(int id, [FromBody] TurmaCreateRequest request)
    {
        try
        {
            var turma = await _turmaService.AtualizarAsync(id, request);
            return Ok(turma);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{turmaId}/matriculas")]
    public async Task<IActionResult> Matricular(int turmaId, [FromBody] MatriculaRequest request)
    {
        try
        {
            await _turmaService.MatricularAlunoAsync(request);
            return Created();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpGet("{turmaId}/matriculas")]
    public async Task<ActionResult<IEnumerable<MatriculaResponse>>> GetMatriculas(int turmaId)
    {
        var matriculas = await _turmaService.GetMatriculasByTurmaAsync(turmaId);
        return Ok(matriculas);
    }

    [HttpDelete("{turmaId}/matriculas/{alunoId}")]
    public async Task<IActionResult> CancelarMatricula(int turmaId, int alunoId)
    {
        try
        {
            await _turmaService.CancelarMatriculaAsync(turmaId, alunoId);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpGet("{turmaId}/disciplinas")]
    public async Task<ActionResult<object>> GetDisciplinasDaTurma(int turmaId)
    {
        var disciplinas = await _turmaService.GetDisciplinasDaTurmaAsync(turmaId);
        return Ok(disciplinas);
    }

    [HttpPut("{turmaId}/disciplinas/{disciplinaId}/docente")]
    public async Task<IActionResult> AtribuirDocente(int turmaId, int disciplinaId, [FromBody] AtribuirDocenteRequest request)
    {
        try
        {
            await _turmaService.AtribuirDocenteAsync(turmaId, disciplinaId, request.DocenteId);
            return Ok(new { message = "Docente atribuído com sucesso." });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}

public record AtribuirDocenteRequest(int? DocenteId);
