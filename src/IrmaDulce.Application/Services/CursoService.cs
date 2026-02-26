using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.Domain.Entities;
using IrmaDulce.Domain.Interfaces;

namespace IrmaDulce.Application.Services;

public class CursoService : ICursoService
{
    private readonly ICursoRepository _cursoRepo;
    private readonly IDisciplinaRepository _disciplinaRepo;
    private readonly IRepository<DisciplinaCurso> _dcRepo;

    public CursoService(ICursoRepository cursoRepo, IDisciplinaRepository disciplinaRepo, IRepository<DisciplinaCurso> dcRepo)
    {
        _cursoRepo = cursoRepo;
        _disciplinaRepo = disciplinaRepo;
        _dcRepo = dcRepo;
    }

    public async Task<CursoResponse> CriarAsync(CursoRequest request)
    {
        // Gera ID funcional: C + sequencial
        var cursos = await _cursoRepo.GetAllAsync();
        var nextId = cursos.Count() + 1;
        var idFuncional = $"C{nextId.ToString().PadLeft(4, '0')}";

        var curso = new Curso
        {
            IdFuncional = idFuncional,
            Nome = request.Nome,
            CargaHoraria = request.CargaHoraria,
        };

        await _cursoRepo.AddAsync(curso);
        return MapToResponse(curso);
    }

    public async Task<CursoResponse?> GetByIdAsync(int id)
    {
        var curso = await _cursoRepo.GetByIdAsync(id);
        return curso == null ? null : MapToResponse(curso);
    }

    public async Task<IEnumerable<CursoResponse>> GetAllAsync()
    {
        var cursos = await _cursoRepo.FindAsync(c => c.Ativo);
        return cursos.Select(MapToResponse);
    }

    public async Task<CursoResponse> AtualizarAsync(int id, CursoRequest request)
    {
        var curso = await _cursoRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Curso com ID {id} não encontrado.");

        curso.Nome = request.Nome;
        curso.CargaHoraria = request.CargaHoraria;

        await _cursoRepo.UpdateAsync(curso);
        return MapToResponse(curso);
    }

    public async Task DeletarAsync(int id)
    {
        var curso = await _cursoRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Curso com ID {id} não encontrado.");

        curso.Ativo = false;
        await _cursoRepo.UpdateAsync(curso);
    }

    public async Task VincularDisciplinaAsync(int cursoId, int disciplinaId, int? semestre)
    {
        var curso = await _cursoRepo.GetByIdAsync(cursoId)
            ?? throw new KeyNotFoundException($"Curso com ID {cursoId} não encontrado.");

        var disciplina = await _disciplinaRepo.GetByIdAsync(disciplinaId)
            ?? throw new KeyNotFoundException($"Disciplina com ID {disciplinaId} não encontrada.");

        // Verifica se já existe
        var existente = await _dcRepo.FindAsync(dc =>
            dc.CursoId == cursoId && dc.DisciplinaId == disciplinaId);

        if (existente.Any())
            throw new InvalidOperationException("Esta disciplina já está vinculada a este curso.");

        var dc = new DisciplinaCurso
        {
            CursoId = cursoId,
            DisciplinaId = disciplinaId,
            Semestre = semestre,
        };

        await _dcRepo.AddAsync(dc);
    }

    public async Task DesvincularDisciplinaAsync(int cursoId, int disciplinaId)
    {
        var dcs = await _dcRepo.FindAsync(dc =>
            dc.CursoId == cursoId && dc.DisciplinaId == disciplinaId);

        var dc = dcs.FirstOrDefault()
            ?? throw new KeyNotFoundException("Vínculo não encontrado.");

        await _dcRepo.DeleteAsync(dc);
    }

    public async Task<IEnumerable<int>> GetDisciplinaIdsDoCursoAsync(int cursoId)
    {
        var dcs = await _dcRepo.FindAsync(dc => dc.CursoId == cursoId);
        return dcs.Select(dc => dc.DisciplinaId);
    }

    private static CursoResponse MapToResponse(Curso c) => new(
        Id: c.Id,
        IdFuncional: c.IdFuncional,
        Nome: c.Nome,
        CargaHoraria: c.CargaHoraria,
        Ativo: c.Ativo
    );
}
