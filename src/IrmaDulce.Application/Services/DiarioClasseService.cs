using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.Domain.Entities;
using IrmaDulce.Domain.Enums;
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
    private readonly IMatriculaRepository _matriculaRepo;

    public DiarioClasseService(
        IDiarioClasseRepository diarioRepo,
        IAvaliacaoRepository avaliacaoRepo,
        INotaAlunoRepository notaRepo,
        IPresencaAlunoRepository presencaRepo,
        IConfiguracaoEscolarRepository configRepo,
        IRepository<PresencaAluno> presencaBaseRepo,
        IMatriculaRepository matriculaRepo)
    {
        _diarioRepo = diarioRepo;
        _avaliacaoRepo = avaliacaoRepo;
        _notaRepo = notaRepo;
        _presencaRepo = presencaRepo;
        _configRepo = configRepo;
        _presencaBaseRepo = presencaBaseRepo;
        _matriculaRepo = matriculaRepo;
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

    public async Task<object> GetHistoricoAsync(int turmaId, int disciplinaId)
    {
        var diarios = await _diarioRepo.GetByTurmaAndDisciplinaAsync(turmaId, disciplinaId);
        var diarioList = diarios.ToList();

        // Build date columns
        var aulas = diarioList.Select(d => new
        {
            d.Id,
            d.Data,
            d.QuantidadeHorasAula,
            ConteudoMinistrado = d.ConteudoMinistrado ?? ""
        }).ToList();

        // Collect all unique student IDs from presences
        var alunoIds = diarioList.SelectMany(d => d.Presencas)
            .Select(p => p.AlunoId)
            .Distinct()
            .ToList();

        // Build student rows with presence per diary entry
        var alunosGrid = alunoIds.Select(alunoId =>
        {
            var firstPresenca = diarioList.SelectMany(d => d.Presencas).FirstOrDefault(p => p.AlunoId == alunoId);
            return new
            {
                AlunoId = alunoId,
                AlunoNome = firstPresenca?.Aluno?.NomeCompleto ?? $"Aluno #{alunoId}",
                AlunoIdFuncional = firstPresenca?.Aluno?.IdFuncional ?? "",
                Presencas = diarioList.Select(d =>
                {
                    var p = d.Presencas.FirstOrDefault(p => p.AlunoId == alunoId);
                    return new
                    {
                        DiarioId = d.Id,
                        Presente = p?.Presente ?? false,
                        FaltaJustificada = p?.FaltaJustificada ?? false,
                    };
                }).ToList()
            };
        }).OrderBy(a => a.AlunoNome).ToList();

        return new { aulas, alunos = alunosGrid };
    }

    public async Task<object> GetNotasGridAsync(int turmaId, int disciplinaId)
    {
        var avaliacoes = (await _avaliacaoRepo.GetByTurmaAndDisciplinaAsync(turmaId, disciplinaId))
            .OrderBy(a => a.DataAplicacao ?? DateTime.MaxValue).ToList();

        var matriculas = (await _matriculaRepo.GetByTurmaIdAsync(turmaId))
            .Where(m => m.Status == StatusMatricula.Ativo).ToList();

        var config = await _configRepo.GetConfigAsync();

        var alunosGrid = new List<object>();
        foreach (var mat in matriculas)
        {
            var notas = (await _notaRepo.GetByAlunoAndDisciplinaAsync(mat.AlunoId, turmaId, disciplinaId)).ToList();

            var notasPorAvaliacao = avaliacoes.Select(av =>
            {
                var nota = notas.FirstOrDefault(n => n.AvaliacaoId == av.Id);
                return new { AvaliacaoId = av.Id, Nota = nota?.Nota, NotaId = nota?.Id };
            }).ToList();

            // Weighted average
            decimal media = 0;
            var somaNxP = notas.Sum(n => n.Nota * n.Avaliacao.Peso);
            var somaP = notas.Sum(n => n.Avaliacao.Peso);
            if (somaP > 0) media = Math.Round(somaNxP / somaP, 2);

            alunosGrid.Add(new
            {
                AlunoId = mat.AlunoId,
                AlunoNome = mat.Aluno.NomeCompleto,
                AlunoIdFuncional = mat.Aluno.IdFuncional,
                Notas = notasPorAvaliacao,
                Media = media,
                Aprovado = media >= config.MediaMinimaAprovacao,
            });
        }

        return new
        {
            avaliacoes = avaliacoes.Select(a => new { a.Id, a.Nome, a.Descricao, a.DataAplicacao, a.Peso }),
            alunos = alunosGrid.OrderBy(a => ((dynamic)a).AlunoNome),
            mediaMinima = config.MediaMinimaAprovacao,
        };
    }
}
