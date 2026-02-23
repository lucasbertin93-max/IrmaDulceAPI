using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.Domain.Entities;
using IrmaDulce.Domain.Interfaces;

namespace IrmaDulce.Application.Services;

public class DiarioClasseService : IDiarioClasseService
{
    private readonly IDiarioClasseRepository _diarioRepo;
    private readonly IAvaliacaoRepository _avaliacaoRepo;
    private readonly INotaAlunoRepository _notaRepo;
    private readonly IPresencaAlunoRepository _presencaRepo;
    private readonly IConfiguracaoEscolarRepository _configRepo;
    private readonly IRepository<PresencaAluno> _presencaBaseRepo;

    public DiarioClasseService(
        IDiarioClasseRepository diarioRepo,
        IAvaliacaoRepository avaliacaoRepo,
        INotaAlunoRepository notaRepo,
        IPresencaAlunoRepository presencaRepo,
        IConfiguracaoEscolarRepository configRepo,
        IRepository<PresencaAluno> presencaBaseRepo)
    {
        _diarioRepo = diarioRepo;
        _avaliacaoRepo = avaliacaoRepo;
        _notaRepo = notaRepo;
        _presencaRepo = presencaRepo;
        _configRepo = configRepo;
        _presencaBaseRepo = presencaBaseRepo;
    }

    public async Task<int> RegistrarAulaAsync(DiarioClasseRequest request)
    {
        var diario = new DiarioClasse
        {
            Data = request.Data,
            DocenteId = request.DocenteId,
            TurmaId = request.TurmaId,
            DisciplinaId = request.DisciplinaId,
            QuantidadeHorasAula = request.QuantidadeHorasAula,
            ConteudoMinistrado = request.ConteudoMinistrado,
            Observacoes = request.Observacoes,
        };

        await _diarioRepo.AddAsync(diario);

        // Se presenças vieram no request, registra
        if (request.Presencas != null && request.Presencas.Any())
        {
            await RegistrarPresencasAsync(diario.Id, request.Presencas);
        }

        return diario.Id;
    }

    public async Task RegistrarPresencasAsync(int diarioId, List<PresencaRequest> presencas)
    {
        foreach (var p in presencas)
        {
            var presenca = new PresencaAluno
            {
                AlunoId = p.AlunoId,
                DiarioClasseId = diarioId,
                Presente = p.Presente,
                FaltaJustificada = p.FaltaJustificada,
                Justificativa = p.Justificativa,
            };

            await _presencaBaseRepo.AddAsync(presenca);
        }
    }

    public async Task LancarNotaAsync(NotaRequest request)
    {
        // Verifica se já existe nota do aluno para esta avaliação
        var existentes = await _notaRepo.FindAsync(n =>
            n.AlunoId == request.AlunoId && n.AvaliacaoId == request.AvaliacaoId);

        if (existentes.Any())
        {
            // Atualiza a nota existente
            var existing = existentes.First();
            existing.Nota = request.Nota;
            existing.Observacao = request.Observacao;
            await _notaRepo.UpdateAsync(existing);
        }
        else
        {
            var nota = new NotaAluno
            {
                AlunoId = request.AlunoId,
                AvaliacaoId = request.AvaliacaoId,
                Nota = request.Nota,
                Observacao = request.Observacao,
            };
            await _notaRepo.AddAsync(nota);
        }
    }

    public async Task<IEnumerable<NotaAlunoResumo>> GetNotasByAlunoAsync(int alunoId, int turmaId, int disciplinaId)
    {
        var notas = await _notaRepo.GetByAlunoAndDisciplinaAsync(alunoId, turmaId, disciplinaId);
        return notas.Select(n => new NotaAlunoResumo(
            AvaliacaoNome: n.Avaliacao.Nome,
            Nota: n.Nota,
            Peso: n.Avaliacao.Peso
        ));
    }

    /// <summary>
    /// Calcula a média ponderada: M = Σ(Ni × Pi) / Σ(Pi)
    /// Regra de negócio 6.1
    /// </summary>
    public async Task<decimal> CalcularMediaAsync(int alunoId, int turmaId, int disciplinaId)
    {
        var notas = await _notaRepo.GetByAlunoAndDisciplinaAsync(alunoId, turmaId, disciplinaId);

        if (!notas.Any()) return 0;

        var somaNxP = notas.Sum(n => n.Nota * n.Avaliacao.Peso);
        var somaP = notas.Sum(n => n.Avaliacao.Peso);

        return somaP == 0 ? 0 : Math.Round(somaNxP / somaP, 2);
    }

    /// <summary>
    /// Calcula a frequência: (Horas presentes + justificadas) / Total horas × 100
    /// Regra de negócio 6.2
    /// </summary>
    public async Task<decimal> CalcularFrequenciaAsync(int alunoId, int turmaId, int disciplinaId)
    {
        var diarios = await _diarioRepo.GetByTurmaAndDisciplinaAsync(turmaId, disciplinaId);

        if (!diarios.Any()) return 100;

        var totalHorasMinistradas = diarios.Sum(d => d.QuantidadeHorasAula);

        var presencas = await _presencaRepo.GetByAlunoAndDisciplinaAsync(alunoId, turmaId, disciplinaId);

        // Horas presentes = soma das horas-aula dos dias em que esteve presente OU falta justificada
        decimal horasPresentes = 0;
        foreach (var p in presencas)
        {
            if (p.Presente || p.FaltaJustificada)
            {
                horasPresentes += p.DiarioClasse.QuantidadeHorasAula;
            }
        }

        return totalHorasMinistradas == 0
            ? 100
            : Math.Round((horasPresentes / totalHorasMinistradas) * 100, 2);
    }

    /// <summary>
    /// Verifica aprovação: Média >= MédiaMinima E Frequência >= FrequênciaMinima
    /// Regra de negócio 6.3
    /// </summary>
    public async Task<bool> AlunoAprovadoAsync(int alunoId, int turmaId, int disciplinaId)
    {
        var config = await _configRepo.GetConfigAsync();
        var media = await CalcularMediaAsync(alunoId, turmaId, disciplinaId);
        var frequencia = await CalcularFrequenciaAsync(alunoId, turmaId, disciplinaId);

        return media >= config.MediaMinimaAprovacao && frequencia >= config.FrequenciaMinimaPercent;
    }
}
