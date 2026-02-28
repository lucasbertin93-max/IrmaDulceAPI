using IrmaDulce.Domain.Entities;

namespace IrmaDulce.Domain.Interfaces;

public interface IPessoaRepository : IRepository<Pessoa>
{
    Task<Pessoa?> GetByCpfAsync(string cpf);
    Task<Pessoa?> GetByIdFuncionalAsync(string idFuncional);
    Task<int> GetNextSequentialIdAsync(string prefix);
    Task<Pessoa?> GetByIdWithResponsavelAsync(int id);
}

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetByLoginAsync(string login);
    Task<Usuario?> GetByPessoaIdAsync(int pessoaId);
}

public interface ICursoRepository : IRepository<Curso>
{
    Task<Curso?> GetWithDisciplinasAsync(int id);
}

public interface IDisciplinaRepository : IRepository<Disciplina> { }

public interface ITurmaRepository : IRepository<Turma>
{
    Task<Turma?> GetWithMatriculasAsync(int id);
    Task<Turma?> GetWithDisciplinasAsync(int id);
    Task<IEnumerable<Turma>> SearchByNameAsync(string searchTerm);
}

public interface IMatriculaRepository : IRepository<Matricula>
{
    Task<IEnumerable<Matricula>> GetByAlunoIdAsync(int alunoId);
    Task<IEnumerable<Matricula>> GetByTurmaIdAsync(int turmaId);
}

public interface IDiarioClasseRepository : IRepository<DiarioClasse>
{
    Task<DiarioClasse?> GetWithPresencasAsync(int id);
    Task<IEnumerable<DiarioClasse>> GetByTurmaAndDisciplinaAsync(int turmaId, int disciplinaId);
}

public interface IAvaliacaoRepository : IRepository<Avaliacao>
{
    Task<IEnumerable<Avaliacao>> GetByTurmaAndDisciplinaAsync(int turmaId, int disciplinaId);
}

public interface INotaAlunoRepository : IRepository<NotaAluno>
{
    Task<IEnumerable<NotaAluno>> GetByAlunoAndDisciplinaAsync(int alunoId, int turmaId, int disciplinaId);
}

public interface IPresencaAlunoRepository : IRepository<PresencaAluno>
{
    Task<IEnumerable<PresencaAluno>> GetByAlunoAndDisciplinaAsync(int alunoId, int turmaId, int disciplinaId);
}

public interface IMensalidadeRepository : IRepository<Mensalidade>
{
    Task<IEnumerable<Mensalidade>> GetByAlunoIdAsync(int alunoId);
    Task<IEnumerable<Mensalidade>> GetAtrasadasAsync();
    Task<bool> AlunoInadimplente(int alunoId);
}

public interface IPagamentoEscolaRepository : IRepository<PagamentoEscola> { }

public interface ILancamentoFinanceiroRepository : IRepository<LancamentoFinanceiro>
{
    Task<IEnumerable<LancamentoFinanceiro>> GetByPeriodoAsync(DateTime inicio, DateTime fim);
}

public interface ICategoriaFinanceiraRepository : IRepository<CategoriaFinanceira> { }

public interface ICronogramaAulaRepository : IRepository<CronogramaAula>
{
    Task<IEnumerable<CronogramaAula>> GetByDocenteAndDataAsync(int docenteId, DateTime data);
    Task<IEnumerable<CronogramaAula>> GetByTurmaAndDataAsync(int turmaId, DateTime data);
    Task<bool> ExisteConflitoDocenteAsync(int docenteId, DateTime data, TimeSpan horaInicio, TimeSpan horaFim, int? excludeId = null);
    Task<bool> ExisteConflitoTurmaAsync(int turmaId, DateTime data, TimeSpan horaInicio, TimeSpan horaFim, int? excludeId = null);
}

public interface IConfiguracaoEscolarRepository : IRepository<ConfiguracaoEscolar>
{
    Task<ConfiguracaoEscolar> GetConfigAsync();
}

public interface ITemplateDocumentoRepository : IRepository<TemplateDocumento>
{
    Task<TemplateDocumento?> GetByTipoAsync(IrmaDulce.Domain.Enums.TipoDocumento tipo);
}

public interface ITemplateTagRepository : IRepository<TemplateTag>
{
    Task<IEnumerable<TemplateTag>> GetByTemplateIdAsync(int templateId);
}

