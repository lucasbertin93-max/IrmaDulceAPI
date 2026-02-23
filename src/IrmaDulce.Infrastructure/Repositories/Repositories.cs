using IrmaDulce.Domain.Entities;
using IrmaDulce.Domain.Interfaces;
using IrmaDulce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IrmaDulce.Infrastructure.Repositories;

public class PessoaRepository : Repository<Pessoa>, IPessoaRepository
{
    public PessoaRepository(AppDbContext context) : base(context) { }

    public async Task<Pessoa?> GetByCpfAsync(string cpf)
        => await _dbSet.FirstOrDefaultAsync(p => p.CPF == cpf);

    public async Task<Pessoa?> GetByIdFuncionalAsync(string idFuncional)
        => await _dbSet.FirstOrDefaultAsync(p => p.IdFuncional == idFuncional);

    public async Task<int> GetNextSequentialIdAsync(string prefix)
    {
        var lastPessoa = await _dbSet
            .Where(p => p.IdFuncional.StartsWith(prefix))
            .OrderByDescending(p => p.IdFuncional)
            .FirstOrDefaultAsync();

        if (lastPessoa == null) return 1;

        var numericPart = lastPessoa.IdFuncional.Substring(prefix.Length);
        return int.Parse(numericPart) + 1;
    }
}

public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(AppDbContext context) : base(context) { }

    public async Task<Usuario?> GetByLoginAsync(string login)
        => await _dbSet.Include(u => u.Pessoa).FirstOrDefaultAsync(u => u.Login == login);

    public async Task<Usuario?> GetByPessoaIdAsync(int pessoaId)
        => await _dbSet.FirstOrDefaultAsync(u => u.PessoaId == pessoaId);
}

public class CursoRepository : Repository<Curso>, ICursoRepository
{
    public CursoRepository(AppDbContext context) : base(context) { }

    public async Task<Curso?> GetWithDisciplinasAsync(int id)
        => await _dbSet.Include(c => c.DisciplinaCursos).ThenInclude(dc => dc.Disciplina)
                       .FirstOrDefaultAsync(c => c.Id == id);
}

public class DisciplinaRepository : Repository<Disciplina>, IDisciplinaRepository
{
    public DisciplinaRepository(AppDbContext context) : base(context) { }
}

public class TurmaRepository : Repository<Turma>, ITurmaRepository
{
    public TurmaRepository(AppDbContext context) : base(context) { }

    public async Task<Turma?> GetWithMatriculasAsync(int id)
        => await _dbSet.Include(t => t.Matriculas).ThenInclude(m => m.Aluno)
                       .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Turma?> GetWithDisciplinasAsync(int id)
        => await _dbSet.Include(t => t.TurmaDisciplinas).ThenInclude(td => td.Disciplina)
                       .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<Turma>> SearchByNameAsync(string searchTerm)
        => await _dbSet.Where(t => t.Nome.Contains(searchTerm) || t.IdFuncional.Contains(searchTerm))
                       .ToListAsync();
}

public class MatriculaRepository : Repository<Matricula>, IMatriculaRepository
{
    public MatriculaRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Matricula>> GetByAlunoIdAsync(int alunoId)
        => await _dbSet.Include(m => m.Turma).Where(m => m.AlunoId == alunoId).ToListAsync();

    public async Task<IEnumerable<Matricula>> GetByTurmaIdAsync(int turmaId)
        => await _dbSet.Include(m => m.Aluno).Where(m => m.TurmaId == turmaId).ToListAsync();
}

public class DiarioClasseRepository : Repository<DiarioClasse>, IDiarioClasseRepository
{
    public DiarioClasseRepository(AppDbContext context) : base(context) { }

    public async Task<DiarioClasse?> GetWithPresencasAsync(int id)
        => await _dbSet.Include(d => d.Presencas).ThenInclude(p => p.Aluno)
                       .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<IEnumerable<DiarioClasse>> GetByTurmaAndDisciplinaAsync(int turmaId, int disciplinaId)
        => await _dbSet.Where(d => d.TurmaId == turmaId && d.DisciplinaId == disciplinaId)
                       .Include(d => d.Presencas)
                       .OrderBy(d => d.Data)
                       .ToListAsync();
}

public class AvaliacaoRepository : Repository<Avaliacao>, IAvaliacaoRepository
{
    public AvaliacaoRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Avaliacao>> GetByTurmaAndDisciplinaAsync(int turmaId, int disciplinaId)
        => await _dbSet.Where(a => a.TurmaId == turmaId && a.DisciplinaId == disciplinaId).ToListAsync();
}

public class NotaAlunoRepository : Repository<NotaAluno>, INotaAlunoRepository
{
    public NotaAlunoRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<NotaAluno>> GetByAlunoAndDisciplinaAsync(int alunoId, int turmaId, int disciplinaId)
        => await _dbSet.Include(n => n.Avaliacao)
                       .Where(n => n.AlunoId == alunoId && n.Avaliacao.TurmaId == turmaId && n.Avaliacao.DisciplinaId == disciplinaId)
                       .ToListAsync();
}

public class PresencaAlunoRepository : Repository<PresencaAluno>, IPresencaAlunoRepository
{
    public PresencaAlunoRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<PresencaAluno>> GetByAlunoAndDisciplinaAsync(int alunoId, int turmaId, int disciplinaId)
        => await _dbSet.Include(p => p.DiarioClasse)
                       .Where(p => p.AlunoId == alunoId && p.DiarioClasse.TurmaId == turmaId && p.DiarioClasse.DisciplinaId == disciplinaId)
                       .ToListAsync();
}

public class MensalidadeRepository : Repository<Mensalidade>, IMensalidadeRepository
{
    public MensalidadeRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Mensalidade>> GetByAlunoIdAsync(int alunoId)
        => await _dbSet.Where(m => m.AlunoId == alunoId).OrderByDescending(m => m.AnoReferencia).ThenByDescending(m => m.MesReferencia).ToListAsync();

    public async Task<IEnumerable<Mensalidade>> GetAtrasadasAsync()
        => await _dbSet.Include(m => m.Aluno)
                       .Where(m => m.Status == Domain.Enums.StatusMensalidade.Atrasado)
                       .ToListAsync();

    public async Task<bool> AlunoInadimplente(int alunoId)
        => await _dbSet.AnyAsync(m => m.AlunoId == alunoId && m.Status == Domain.Enums.StatusMensalidade.Atrasado);
}

public class PagamentoEscolaRepository : Repository<PagamentoEscola>, IPagamentoEscolaRepository
{
    public PagamentoEscolaRepository(AppDbContext context) : base(context) { }
}

public class LancamentoFinanceiroRepository : Repository<LancamentoFinanceiro>, ILancamentoFinanceiroRepository
{
    public LancamentoFinanceiroRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<LancamentoFinanceiro>> GetByPeriodoAsync(DateTime inicio, DateTime fim)
        => await _dbSet.Include(l => l.Categoria)
                       .Where(l => l.Data >= inicio && l.Data <= fim)
                       .OrderByDescending(l => l.Data)
                       .ToListAsync();
}

public class CategoriaFinanceiraRepository : Repository<CategoriaFinanceira>, ICategoriaFinanceiraRepository
{
    public CategoriaFinanceiraRepository(AppDbContext context) : base(context) { }
}

public class CronogramaAulaRepository : Repository<CronogramaAula>, ICronogramaAulaRepository
{
    public CronogramaAulaRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<CronogramaAula>> GetByDocenteAndDataAsync(int docenteId, DateTime data)
        => await _dbSet.Include(c => c.Turma).Include(c => c.Disciplina)
                       .Where(c => c.DocenteId == docenteId && c.Data.Date == data.Date)
                       .OrderBy(c => c.HoraInicio).ToListAsync();

    public async Task<IEnumerable<CronogramaAula>> GetByTurmaAndDataAsync(int turmaId, DateTime data)
        => await _dbSet.Include(c => c.Docente).Include(c => c.Disciplina)
                       .Where(c => c.TurmaId == turmaId && c.Data.Date == data.Date)
                       .OrderBy(c => c.HoraInicio).ToListAsync();

    public async Task<bool> ExisteConflitoDocenteAsync(int docenteId, DateTime data, TimeSpan horaInicio, TimeSpan horaFim, int? excludeId = null)
        => await _dbSet.AnyAsync(c =>
            c.DocenteId == docenteId &&
            c.Data.Date == data.Date &&
            c.HoraInicio < horaFim &&
            c.HoraFim > horaInicio &&
            (excludeId == null || c.Id != excludeId));

    public async Task<bool> ExisteConflitoTurmaAsync(int turmaId, DateTime data, TimeSpan horaInicio, TimeSpan horaFim, int? excludeId = null)
        => await _dbSet.AnyAsync(c =>
            c.TurmaId == turmaId &&
            c.Data.Date == data.Date &&
            c.HoraInicio < horaFim &&
            c.HoraFim > horaInicio &&
            (excludeId == null || c.Id != excludeId));
}

public class ConfiguracaoEscolarRepository : Repository<ConfiguracaoEscolar>, IConfiguracaoEscolarRepository
{
    public ConfiguracaoEscolarRepository(AppDbContext context) : base(context) { }

    public async Task<ConfiguracaoEscolar> GetConfigAsync()
        => await _dbSet.FirstAsync();
}

public class TemplateDocumentoRepository : Repository<TemplateDocumento>, ITemplateDocumentoRepository
{
    public TemplateDocumentoRepository(AppDbContext context) : base(context) { }

    public async Task<TemplateDocumento?> GetByTipoAsync(Domain.Enums.TipoDocumento tipo)
        => await _dbSet.FirstOrDefaultAsync(t => t.Tipo == tipo && t.Ativo);
}
