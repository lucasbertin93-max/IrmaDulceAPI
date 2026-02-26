using IrmaDulce.Application.DTOs;
using IrmaDulce.Domain.Enums;

namespace IrmaDulce.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<bool> RecuperarSenhaAsync(string login);
}

public interface IPessoaService
{
    Task<PessoaResponse> CriarAsync(PessoaCreateRequest request);
    Task<PessoaResponse?> GetByIdAsync(int id);
    Task<PessoaResponse?> GetByIdFuncionalAsync(string idFuncional);
    Task<IEnumerable<PessoaResponse>> GetAllAsync(PerfilUsuario? perfil = null);
    Task<PessoaResponse> AtualizarAsync(int id, PessoaCreateRequest request);
    Task DesativarAsync(int id);
}

public interface ICursoService
{
    Task<CursoResponse> CriarAsync(CursoRequest request);
    Task<CursoResponse?> GetByIdAsync(int id);
    Task<IEnumerable<CursoResponse>> GetAllAsync();
    Task<CursoResponse> AtualizarAsync(int id, CursoRequest request);
    Task DeletarAsync(int id);
    Task VincularDisciplinaAsync(int cursoId, int disciplinaId, int? semestre);
    Task DesvincularDisciplinaAsync(int cursoId, int disciplinaId);
    Task<IEnumerable<int>> GetDisciplinaIdsDoCursoAsync(int cursoId);
}

public interface IDisciplinaService
{
    Task<DisciplinaResponse> CriarAsync(DisciplinaRequest request);
    Task<DisciplinaResponse?> GetByIdAsync(int id);
    Task<IEnumerable<DisciplinaResponse>> GetAllAsync();
    Task<DisciplinaResponse> AtualizarAsync(int id, DisciplinaRequest request);
    Task DeletarAsync(int id);
}

public interface ITurmaService
{
    Task<TurmaResponse> CriarAsync(TurmaCreateRequest request);
    Task<TurmaResponse?> GetByIdAsync(int id);
    Task<IEnumerable<TurmaResponse>> GetAllAsync();
    Task<IEnumerable<TurmaResponse>> PesquisarAsync(string searchTerm);
    Task<TurmaResponse> AtualizarAsync(int id, TurmaCreateRequest request);
    Task MatricularAlunoAsync(MatriculaRequest request);
    Task<IEnumerable<MatriculaResponse>> GetMatriculasByTurmaAsync(int turmaId);
    Task CancelarMatriculaAsync(int turmaId, int alunoId);
    Task<object> GetDisciplinasDaTurmaAsync(int turmaId);
    Task AtribuirDocenteAsync(int turmaId, int disciplinaId, int? docenteId);
}

public interface IDiarioClasseService
{
    Task<int> RegistrarAulaAsync(DiarioClasseRequest request);
    Task RegistrarPresencasAsync(int diarioId, List<PresencaRequest> presencas);
    Task LancarNotaAsync(NotaRequest request);
    Task<IEnumerable<NotaAlunoResumo>> GetNotasByAlunoAsync(int alunoId, int turmaId, int disciplinaId);
    Task<decimal> CalcularMediaAsync(int alunoId, int turmaId, int disciplinaId);
    Task<decimal> CalcularFrequenciaAsync(int alunoId, int turmaId, int disciplinaId);
    Task<bool> AlunoAprovadoAsync(int alunoId, int turmaId, int disciplinaId);
    Task<object> GetHistoricoAsync(int turmaId, int disciplinaId);
    Task<object> GetNotasGridAsync(int turmaId, int disciplinaId);
}

public record NotaAlunoResumo(string AvaliacaoNome, decimal Nota, decimal Peso);

public interface IAvaliacaoService
{
    Task<AvaliacaoResponse> CriarAsync(AvaliacaoRequest request);
    Task<IEnumerable<AvaliacaoResponse>> GetByTurmaAndDisciplinaAsync(int turmaId, int disciplinaId);
    Task<AvaliacaoResponse> AtualizarAsync(int id, AvaliacaoRequest request);
    Task DeletarAsync(int id);
}

public interface IDocumentoService
{
    Task<byte[]> EmitirDocumentoAsync(EmitirDocumentoRequest request, int operadorId);
}

public interface IFinanceiroService
{
    Task GerarMensalidadesAsync(GerarMensalidadesRequest request);
    Task<IEnumerable<MensalidadeResponse>> GetMensalidadesAsync(int? alunoId, StatusMensalidade? status, int? mes, int? ano);
    Task RegistrarPagamentoAsync(RegistrarPagamentoRequest request, int operadorId);
    Task<MensalidadeResponse> AtualizarMensalidadeAsync(int id, decimal valor, DateTime dataVencimento);
    Task DeletarMensalidadeAsync(int id);
    Task<LancamentoResponse> AdicionarLancamentoAsync(LancamentoRequest request);
    Task<IEnumerable<LancamentoResponse>> GetLancamentosAsync(DateTime inicio, DateTime fim, TipoLancamento? tipo);
    Task<DashboardFinanceiroResponse> GetDashboardAsync(DateTime inicio, DateTime fim);
}

public interface ICronogramaService
{
    Task<CronogramaResponse> CriarAsync(CronogramaRequest request);
    Task<IEnumerable<CronogramaResponse>> GetByDataAsync(DateTime data);
    Task<IEnumerable<CronogramaResponse>> GetByDocenteAsync(int docenteId, DateTime inicio, DateTime fim);
    Task<CronogramaResponse> AtualizarAsync(int id, CronogramaRequest request);
    Task DeletarAsync(int id);
    Task<IEnumerable<ConflitoCronogramaResponse>> VerificarConflitosAsync(CronogramaRequest request, int? excludeId = null);
}

public interface IConfiguracaoService
{
    Task<ConfiguracaoResponse> GetAsync();
    Task<ConfiguracaoResponse> AtualizarAsync(ConfiguracaoRequest request);
}
