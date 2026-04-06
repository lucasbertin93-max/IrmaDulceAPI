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
    private readonly ICursoRepository _cursoRepo;
    private readonly IRepository<TurmaDisciplinaHorario> _horarioRepo;

    public CronogramaService(
        ICronogramaAulaRepository cronogramaRepo,
        IPessoaRepository pessoaRepo,
        ITurmaRepository turmaRepo,
        IDisciplinaRepository disciplinaRepo,
        ICursoRepository cursoRepo,
        IRepository<TurmaDisciplinaHorario> horarioRepo)
    {
        _cronogramaRepo = cronogramaRepo;
        _pessoaRepo = pessoaRepo;
        _turmaRepo = turmaRepo;
        _disciplinaRepo = disciplinaRepo;
        _cursoRepo = cursoRepo;
        _horarioRepo = horarioRepo;
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

    public async Task<CronogramaGerarLoteResponse> GerarLoteAsync(int turmaId, DateTime dataInicio, DateTime dataFim)
    {
        var conflitos = new List<ConflitoCronogramaResponse>();
        var aulasGeradasCount = 0;

        // 1. Carregar Turma com Dias Letivos
        var turma = await _turmaRepo.GetWithDiasLetivosAsync(turmaId)
            ?? throw new KeyNotFoundException($"Turma com ID {turmaId} não encontrada.");

        if (!turma.DiasLetivos.Any())
            throw new InvalidOperationException("A Turma não possui Dias Letivos configurados. Não é possível gerar cronograma automático.");

        // 2. Carregar Disciplinas da Turma ordenadas pela Ordem do Curso
        // Precisamos buscar as disciplinas vinculadas ao Curso da Turma
        var curso = await _cursoRepo.GetWithDisciplinasAsync(turma.CursoId);
        if (curso == null || !curso.DisciplinaCursos.Any())
            throw new InvalidOperationException("O Curso da turma não possui disciplinas vinculadas.");

        // Obter vínculos de professores para as disciplinas desta turma
        var turmaCompleta = await _turmaRepo.GetWithDisciplinasAsync(turmaId);
        
        var filaDisciplinas = curso.DisciplinaCursos
            .OrderBy(dc => dc.Ordem)
            .Select(dc => new 
            {
                Disciplina = dc.Disciplina,
                VinculoTurma = turmaCompleta?.TurmaDisciplinas.FirstOrDefault(td => td.DisciplinaId == dc.DisciplinaId)
            })
            .Where(x => x.VinculoTurma != null && x.VinculoTurma.DocenteId != null) // Só podemos gerar se houver docente atribuído
            .ToList();

        if (!filaDisciplinas.Any())
            throw new InvalidOperationException("A Turma não possui disciplinas com docentes atribuídos para gerar aulas.");

        // 3. Iterar pelos dias do período
        var dataAtual = dataInicio.Date;
        
        // Cache de horas das disciplinas e disponibilidade
        var cargaHorariaAtual = new Dictionary<int, int>();
        foreach (var fx in filaDisciplinas)
        {
            cargaHorariaAtual[fx.Disciplina.Id] = await _cronogramaRepo.GetTotalHorasLecionadasAsync(turmaId, fx.Disciplina.Id);
        }

        var disponibilidadeCache = new Dictionary<int, List<DisponibilidadeDocente>>();

        var tdIds = filaDisciplinas.Select(x => x.VinculoTurma!.Id).ToList();
        var todosHorarios = await _horarioRepo.GetAllAsync();
        var horariosTurma = todosHorarios.Where(h => tdIds.Contains(h.TurmaDisciplinaId)).ToList();

        while (dataAtual <= dataFim.Date)
        {
            var diaSemana = dataAtual.DayOfWeek;

            // Dia letivo da turma?
            var diaLetivo = turma.DiasLetivos.FirstOrDefault(d => d.DiaSemana == diaSemana);
            if (diaLetivo != null)
            {
                var duracaoTotal = diaLetivo.HoraFim - diaLetivo.HoraInicio;
                var meioTurno = TimeSpan.FromTicks(duracaoTotal.Ticks / 2);

                var slots = new[]
                {
                    new { Turno = 1, Inicio = diaLetivo.HoraInicio, Fim = diaLetivo.HoraInicio + meioTurno },
                    new { Turno = 2, Inicio = diaLetivo.HoraInicio + meioTurno, Fim = diaLetivo.HoraFim }
                };

                foreach (var slot in slots)
                {
                    // Tenta achar disciplina para este slot
                    foreach (var item in filaDisciplinas)
                    {
                        if (cargaHorariaAtual[item.Disciplina.Id] >= item.Disciplina.CargaHoraria)
                            continue; // Já cumpriu a carga horária
                            
                        // Verificar se esta disciplina tem este Slot/Dia configurado
                        var configsHorario = horariosTurma.Where(x => x.TurmaDisciplinaId == item.VinculoTurma!.Id).ToList();
                        
                        // Se a disciplina tem configurações de horário, ela SÓ PODE RODAR naqueles horários
                        if (configsHorario.Any())
                        {
                            if (!configsHorario.Any(h => h.DiaSemana == diaSemana && h.Turno == slot.Turno))
                                continue; 
                        }

                        var docenteId = item.VinculoTurma!.DocenteId!.Value;

                        // Carrega disponibilidade se não tiver no cache
                        if (!disponibilidadeCache.ContainsKey(docenteId))
                        {
                            var pessoaObj = await _pessoaRepo.GetWithDisponibilidadeAsync(docenteId);
                            disponibilidadeCache[docenteId] = pessoaObj?.Disponibilidades.ToList() ?? new List<DisponibilidadeDocente>();
                        }

                        var disponibilidadesDocente = disponibilidadeCache[docenteId];
                        var docenteDisponivelHoje = disponibilidadesDocente.Any(d => d.DiaSemana == diaSemana && d.HoraInicio <= slot.Inicio && d.HoraFim >= slot.Fim);

                        if (!docenteDisponivelHoje)
                            continue;

                        // Verifica se o docente já não tem aula agendada (em outra turma)
                        var conflito = await _cronogramaRepo.ExisteConflitoDocenteAsync(docenteId, dataAtual, slot.Inicio, slot.Fim);
                        if (conflito)
                        {
                            conflitos.Add(new ConflitoCronogramaResponse("Docente Indisponível", $"Professor ID {docenteId} já alocado na data {dataAtual:dd/MM/yyyy} {slot.Inicio}-{slot.Fim}"));
                            continue; // Tenta próxima disciplina
                        }

                        // Se passou nas validações, agenda!
                        var novaAula = new CronogramaAula
                        {
                            TurmaId = turma.Id,
                            DisciplinaId = item.Disciplina.Id,
                            DocenteId = docenteId,
                            Data = dataAtual,
                            HoraInicio = slot.Inicio,
                            HoraFim = slot.Fim,
                            IsEstagio = item.Disciplina.IsEstagio
                        };

                        await _cronogramaRepo.AddAsync(novaAula);
                        aulasGeradasCount++;
                        
                        var duracaoEmHoras = Math.Max(1, (int)(slot.Fim - slot.Inicio).TotalHours);
                        cargaHorariaAtual[item.Disciplina.Id] += duracaoEmHoras;

                        // Se alocamos 1 disciplina neste Slot (Pre intervalo ou Pos), encerra a busca para este slot.
                        break;
                    }
                }
            }

            dataAtual = dataAtual.AddDays(1);
        }

        return new CronogramaGerarLoteResponse(TotalAulasGeradas: aulasGeradasCount, Conflitos: conflitos);
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
            Sala: c.Sala,
            IsEstagio: c.IsEstagio
        );
    }
}
