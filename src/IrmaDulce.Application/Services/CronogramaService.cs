using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.Domain.Entities;
using IrmaDulce.Domain.Interfaces;

namespace IrmaDulce.Application.Services;

public class CronogramaService : ICronogramaService
{
    private readonly ICronogramaAulaRepository _cronogramaRepo;
    private readonly IPessoaRepository _pessoaRepo;
    private readonly ITurmaRepository _turmaRepo;
    private readonly IDisciplinaRepository _disciplinaRepo;

    public CronogramaService(
        ICronogramaAulaRepository cronogramaRepo,
        IPessoaRepository pessoaRepo,
        ITurmaRepository turmaRepo,
        IDisciplinaRepository disciplinaRepo)
    {
        _cronogramaRepo = cronogramaRepo;
        _pessoaRepo = pessoaRepo;
        _turmaRepo = turmaRepo;
        _disciplinaRepo = disciplinaRepo;
    }

    public async Task<CronogramaResponse> CriarAsync(CronogramaRequest request)
    {
        var cronograma = new CronogramaAula
        {
            TurmaId = request.TurmaId,
            DisciplinaId = request.DisciplinaId,
            DocenteId = request.DocenteId,
            Data = request.Data,
            HoraInicio = request.HoraInicio,
            HoraFim = request.HoraFim,
            Sala = request.Sala,
        };

        await _cronogramaRepo.AddAsync(cronograma);
        return await MapToResponseAsync(cronograma);
    }

    public async Task<IEnumerable<CronogramaResponse>> GetByDataAsync(DateTime data)
    {
        // Busca cronogramas de todas as turmas do dia
        var todos = await _cronogramaRepo.FindAsync(c => c.Data.Date == data.Date);
        var result = new List<CronogramaResponse>();
        foreach (var c in todos)
            result.Add(await MapToResponseAsync(c));
        return result;
    }

    public async Task<IEnumerable<CronogramaResponse>> GetByDocenteAsync(int docenteId, DateTime inicio, DateTime fim)
    {
        var cronogramas = await _cronogramaRepo.FindAsync(c =>
            c.DocenteId == docenteId && c.Data >= inicio && c.Data <= fim);

        var result = new List<CronogramaResponse>();
        foreach (var c in cronogramas.OrderBy(c => c.Data).ThenBy(c => c.HoraInicio))
            result.Add(await MapToResponseAsync(c));
        return result;
    }

    public async Task<CronogramaResponse> AtualizarAsync(int id, CronogramaRequest request)
    {
        var cronograma = await _cronogramaRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Cronograma com ID {id} não encontrado.");

        cronograma.TurmaId = request.TurmaId;
        cronograma.DisciplinaId = request.DisciplinaId;
        cronograma.DocenteId = request.DocenteId;
        cronograma.Data = request.Data;
        cronograma.HoraInicio = request.HoraInicio;
        cronograma.HoraFim = request.HoraFim;
        cronograma.Sala = request.Sala;

        await _cronogramaRepo.UpdateAsync(cronograma);
        return await MapToResponseAsync(cronograma);
    }

    public async Task DeletarAsync(int id)
    {
        var cronograma = await _cronogramaRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Cronograma com ID {id} não encontrado.");

        await _cronogramaRepo.DeleteAsync(cronograma);
    }

    /// <summary>
    /// Verifica conflitos de agendamento.
    /// Regra de negócio 9.2: alertar (não bloquear) em caso de conflito.
    /// </summary>
    public async Task<IEnumerable<ConflitoCronogramaResponse>> VerificarConflitosAsync(
        CronogramaRequest request, int? excludeId = null)
    {
        var conflitos = new List<ConflitoCronogramaResponse>();

        // Conflito de docente: mesmo professor em duas turmas no mesmo horário/data
        var conflitoDocente = await _cronogramaRepo.ExisteConflitoDocenteAsync(
            request.DocenteId, request.Data, request.HoraInicio, request.HoraFim, excludeId);

        if (conflitoDocente)
        {
            var docente = await _pessoaRepo.GetByIdAsync(request.DocenteId);
            conflitos.Add(new ConflitoCronogramaResponse(
                Tipo: "Docente Duplicado",
                Mensagem: $"O docente {docente?.NomeCompleto ?? "ID " + request.DocenteId} já possui aula agendada neste horário ({request.Data:dd/MM/yyyy} {request.HoraInicio:hh\\:mm}-{request.HoraFim:hh\\:mm})."
            ));
        }

        // Conflito de turma: mesma turma com duas disciplinas no mesmo horário/data
        var conflitoTurma = await _cronogramaRepo.ExisteConflitoTurmaAsync(
            request.TurmaId, request.Data, request.HoraInicio, request.HoraFim, excludeId);

        if (conflitoTurma)
        {
            var turma = await _turmaRepo.GetByIdAsync(request.TurmaId);
            conflitos.Add(new ConflitoCronogramaResponse(
                Tipo: "Turma Duplicada",
                Mensagem: $"A turma {turma?.Nome ?? "ID " + request.TurmaId} já possui aula agendada neste horário ({request.Data:dd/MM/yyyy} {request.HoraInicio:hh\\:mm}-{request.HoraFim:hh\\:mm})."
            ));
        }

        return conflitos;
    }

    private async Task<CronogramaResponse> MapToResponseAsync(CronogramaAula c)
    {
        var turma = await _turmaRepo.GetByIdAsync(c.TurmaId);
        var disciplina = await _disciplinaRepo.GetByIdAsync(c.DisciplinaId);
        var docente = await _pessoaRepo.GetByIdAsync(c.DocenteId);

        return new CronogramaResponse(
            Id: c.Id,
            TurmaId: c.TurmaId,
            TurmaNome: turma?.Nome ?? "",
            DisciplinaId: c.DisciplinaId,
            DisciplinaNome: disciplina?.Nome ?? "",
            DocenteId: c.DocenteId,
            DocenteNome: docente?.NomeCompleto ?? "",
            Data: c.Data,
            HoraInicio: c.HoraInicio,
            HoraFim: c.HoraFim,
            Sala: c.Sala
        );
    }
}
