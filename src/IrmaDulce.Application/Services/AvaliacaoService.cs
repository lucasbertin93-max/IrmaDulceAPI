using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.Domain.Entities;
using IrmaDulce.Domain.Interfaces;

namespace IrmaDulce.Application.Services;

public class AvaliacaoService : IAvaliacaoService
{
    private readonly IAvaliacaoRepository _avaliacaoRepo;

    public AvaliacaoService(IAvaliacaoRepository avaliacaoRepo)
    {
        _avaliacaoRepo = avaliacaoRepo;
    }

    public async Task<AvaliacaoResponse> CriarAsync(AvaliacaoRequest request)
    {
        var avaliacao = new Avaliacao
        {
            Nome = request.Nome,
            Descricao = request.Descricao,
            DataAplicacao = request.DataAplicacao,
            Peso = request.Peso,
            TurmaId = request.TurmaId,
            DisciplinaId = request.DisciplinaId,
        };

        await _avaliacaoRepo.AddAsync(avaliacao);
        return MapToResponse(avaliacao);
    }

    public async Task<IEnumerable<AvaliacaoResponse>> GetByTurmaAndDisciplinaAsync(int turmaId, int disciplinaId)
    {
        var avaliacoes = await _avaliacaoRepo.GetByTurmaAndDisciplinaAsync(turmaId, disciplinaId);
        return avaliacoes.Select(MapToResponse);
    }

    public async Task<AvaliacaoResponse> AtualizarAsync(int id, AvaliacaoRequest request)
    {
        var avaliacao = await _avaliacaoRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Avaliação com ID {id} não encontrada.");

        avaliacao.Nome = request.Nome;
        avaliacao.Descricao = request.Descricao;
        avaliacao.DataAplicacao = request.DataAplicacao;
        avaliacao.Peso = request.Peso;

        await _avaliacaoRepo.UpdateAsync(avaliacao);
        return MapToResponse(avaliacao);
    }

    public async Task DeletarAsync(int id)
    {
        var avaliacao = await _avaliacaoRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Avaliação com ID {id} não encontrada.");

        await _avaliacaoRepo.DeleteAsync(avaliacao);
    }

    private static AvaliacaoResponse MapToResponse(Avaliacao a) => new(
        Id: a.Id,
        Nome: a.Nome,
        Descricao: a.Descricao,
        DataAplicacao: a.DataAplicacao,
        Peso: a.Peso,
        TurmaId: a.TurmaId,
        DisciplinaId: a.DisciplinaId
    );
}
