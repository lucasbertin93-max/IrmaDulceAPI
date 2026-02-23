using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.Domain.Entities;
using IrmaDulce.Domain.Enums;
using IrmaDulce.Domain.Interfaces;

namespace IrmaDulce.Application.Services;

public class TurmaService : ITurmaService
{
    private readonly ITurmaRepository _turmaRepo;
    private readonly ICursoRepository _cursoRepo;
    private readonly IMatriculaRepository _matriculaRepo;
    private readonly IPessoaRepository _pessoaRepo;
    private readonly IRepository<TurmaDisciplina> _tdRepo;

    public TurmaService(
        ITurmaRepository turmaRepo,
        ICursoRepository cursoRepo,
        IMatriculaRepository matriculaRepo,
        IPessoaRepository pessoaRepo,
        IRepository<TurmaDisciplina> tdRepo)
    {
        _turmaRepo = turmaRepo;
        _cursoRepo = cursoRepo;
        _matriculaRepo = matriculaRepo;
        _pessoaRepo = pessoaRepo;
        _tdRepo = tdRepo;
    }

    public async Task<TurmaResponse> CriarAsync(TurmaCreateRequest request)
    {
        var curso = await _cursoRepo.GetWithDisciplinasAsync(request.CursoId)
            ?? throw new KeyNotFoundException($"Curso com ID {request.CursoId} não encontrado.");

        // Gera ID funcional
        var turmas = await _turmaRepo.GetAllAsync();
        var nextId = turmas.Count() + 1;
        var idFuncional = $"T{nextId.ToString().PadLeft(4, '0')}";

        var turma = new Turma
        {
            IdFuncional = idFuncional,
            Nome = request.Nome,
            Horario = request.Horario,
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            CursoId = request.CursoId,
        };

        await _turmaRepo.AddAsync(turma);

        // Importa automaticamente as disciplinas do curso para a turma
        foreach (var dc in curso.DisciplinaCursos)
        {
            var td = new TurmaDisciplina
            {
                TurmaId = turma.Id,
                DisciplinaId = dc.DisciplinaId,
            };
            await _tdRepo.AddAsync(td);
        }

        return MapToResponse(turma, curso.Nome);
    }

    public async Task<TurmaResponse?> GetByIdAsync(int id)
    {
        var turma = await _turmaRepo.GetByIdAsync(id);
        if (turma == null) return null;

        var curso = await _cursoRepo.GetByIdAsync(turma.CursoId);
        return MapToResponse(turma, curso?.Nome ?? "");
    }

    public async Task<IEnumerable<TurmaResponse>> GetAllAsync()
    {
        var turmas = await _turmaRepo.FindAsync(t => t.Ativo);
        var result = new List<TurmaResponse>();
        foreach (var t in turmas)
        {
            var curso = await _cursoRepo.GetByIdAsync(t.CursoId);
            result.Add(MapToResponse(t, curso?.Nome ?? ""));
        }
        return result;
    }

    public async Task<IEnumerable<TurmaResponse>> PesquisarAsync(string searchTerm)
    {
        var turmas = await _turmaRepo.SearchByNameAsync(searchTerm);
        var result = new List<TurmaResponse>();
        foreach (var t in turmas)
        {
            var curso = await _cursoRepo.GetByIdAsync(t.CursoId);
            result.Add(MapToResponse(t, curso?.Nome ?? ""));
        }
        return result;
    }

    public async Task<TurmaResponse> AtualizarAsync(int id, TurmaCreateRequest request)
    {
        var turma = await _turmaRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Turma com ID {id} não encontrada.");

        turma.Nome = request.Nome;
        turma.Horario = request.Horario;
        turma.DataInicio = request.DataInicio;
        turma.DataFim = request.DataFim;

        await _turmaRepo.UpdateAsync(turma);

        var curso = await _cursoRepo.GetByIdAsync(turma.CursoId);
        return MapToResponse(turma, curso?.Nome ?? "");
    }

    public async Task MatricularAlunoAsync(MatriculaRequest request)
    {
        var aluno = await _pessoaRepo.GetByIdAsync(request.AlunoId)
            ?? throw new KeyNotFoundException($"Aluno com ID {request.AlunoId} não encontrado.");

        if (aluno.Perfil != PerfilUsuario.Aluno)
            throw new InvalidOperationException("Apenas alunos podem ser matriculados.");

        var turma = await _turmaRepo.GetByIdAsync(request.TurmaId)
            ?? throw new KeyNotFoundException($"Turma com ID {request.TurmaId} não encontrada.");

        // Verifica se já matriculado
        var matriculasExistentes = await _matriculaRepo.GetByTurmaIdAsync(request.TurmaId);
        if (matriculasExistentes.Any(m => m.AlunoId == request.AlunoId && m.Status == StatusMatricula.Ativo))
            throw new InvalidOperationException("Aluno já está matriculado nesta turma.");

        var matricula = new Matricula
        {
            AlunoId = request.AlunoId,
            TurmaId = request.TurmaId,
            Status = StatusMatricula.Ativo,
            DataMatricula = DateTime.UtcNow,
        };

        await _matriculaRepo.AddAsync(matricula);
    }

    public async Task<IEnumerable<MatriculaResponse>> GetMatriculasByTurmaAsync(int turmaId)
    {
        var matriculas = await _matriculaRepo.GetByTurmaIdAsync(turmaId);
        return matriculas.Select(m => new MatriculaResponse(
            Id: m.Id,
            AlunoId: m.AlunoId,
            AlunoNome: m.Aluno.NomeCompleto,
            AlunoIdFuncional: m.Aluno.IdFuncional,
            TurmaId: m.TurmaId,
            TurmaNome: m.Turma?.Nome ?? "",
            Status: m.Status,
            DataMatricula: m.DataMatricula
        ));
    }

    private static TurmaResponse MapToResponse(Turma t, string cursoNome) => new(
        Id: t.Id,
        IdFuncional: t.IdFuncional,
        Nome: t.Nome,
        Horario: t.Horario,
        DataInicio: t.DataInicio,
        DataFim: t.DataFim,
        CursoId: t.CursoId,
        CursoNome: cursoNome,
        Ativo: t.Ativo
    );
}
