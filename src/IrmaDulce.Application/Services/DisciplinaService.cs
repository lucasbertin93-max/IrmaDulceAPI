using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.Domain.Entities;
using IrmaDulce.Domain.Interfaces;

namespace IrmaDulce.Application.Services;

public class DisciplinaService : IDisciplinaService
{
    private readonly IDisciplinaRepository _disciplinaRepo;

    public DisciplinaService(IDisciplinaRepository disciplinaRepo)
    {
        _disciplinaRepo = disciplinaRepo;
    }

    public async Task<DisciplinaResponse> CriarAsync(DisciplinaRequest request)
    {
        var disciplinas = await _disciplinaRepo.GetAllAsync();
        var nextId = disciplinas.Count() + 1;
        var idFuncional = $"DIS{nextId.ToString().PadLeft(4, '0')}";

        var disciplina = new Disciplina
        {
            IdFuncional = idFuncional,
            Nome = request.Nome,
            CargaHoraria = request.CargaHoraria,
            Descricao = request.Descricao,
        };

        await _disciplinaRepo.AddAsync(disciplina);
        return MapToResponse(disciplina);
    }

    public async Task<DisciplinaResponse?> GetByIdAsync(int id)
    {
        var d = await _disciplinaRepo.GetByIdAsync(id);
        return d == null ? null : MapToResponse(d);
    }

    public async Task<IEnumerable<DisciplinaResponse>> GetAllAsync()
    {
        var disciplinas = await _disciplinaRepo.FindAsync(d => d.Ativo);
        return disciplinas.Select(MapToResponse);
    }

    public async Task<DisciplinaResponse> AtualizarAsync(int id, DisciplinaRequest request)
    {
        var d = await _disciplinaRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Disciplina com ID {id} não encontrada.");

        d.Nome = request.Nome;
        d.CargaHoraria = request.CargaHoraria;
        d.Descricao = request.Descricao;

        await _disciplinaRepo.UpdateAsync(d);
        return MapToResponse(d);
    }

    public async Task DeletarAsync(int id)
    {
        var d = await _disciplinaRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Disciplina com ID {id} não encontrada.");

        d.Ativo = false;
        await _disciplinaRepo.UpdateAsync(d);
    }

    private static DisciplinaResponse MapToResponse(Disciplina d) => new(
        Id: d.Id,
        IdFuncional: d.IdFuncional,
        Nome: d.Nome,
        CargaHoraria: d.CargaHoraria,
        Descricao: d.Descricao,
        Ativo: d.Ativo
    );
}
