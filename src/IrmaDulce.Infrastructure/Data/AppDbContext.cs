using IrmaDulce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IrmaDulce.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Pessoas e Autenticação
    public DbSet<Pessoa> Pessoas => Set<Pessoa>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    // Estrutura Acadêmica
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Disciplina> Disciplinas => Set<Disciplina>();
    public DbSet<DisciplinaCurso> DisciplinaCursos => Set<DisciplinaCurso>();
    public DbSet<Turma> Turmas => Set<Turma>();
    public DbSet<TurmaDisciplina> TurmaDisciplinas => Set<TurmaDisciplina>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();

    // Diário de Classe
    public DbSet<DiarioClasse> DiarioClasses => Set<DiarioClasse>();
    public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();
    public DbSet<NotaAluno> NotasAlunos => Set<NotaAluno>();
    public DbSet<PresencaAluno> PresencasAlunos => Set<PresencaAluno>();

    // Financeiro
    public DbSet<Mensalidade> Mensalidades => Set<Mensalidade>();
    public DbSet<PagamentoEscola> PagamentosEscola => Set<PagamentoEscola>();
    public DbSet<LancamentoFinanceiro> LancamentosFinanceiros => Set<LancamentoFinanceiro>();
    public DbSet<CategoriaFinanceira> CategoriasFinanceiras => Set<CategoriaFinanceira>();

    // Cronograma
    public DbSet<CronogramaAula> CronogramaAulas => Set<CronogramaAula>();

    // Configuração
    public DbSet<ConfiguracaoEscolar> ConfiguracoesEscolares => Set<ConfiguracaoEscolar>();
    public DbSet<TemplateDocumento> TemplatesDocumentos => Set<TemplateDocumento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========== Pessoa ==========
        modelBuilder.Entity<Pessoa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CPF).IsUnique();
            entity.HasIndex(e => e.IdFuncional).IsUnique();
            entity.Property(e => e.NomeCompleto).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CPF).HasMaxLength(14).IsRequired();
            entity.Property(e => e.RG).HasMaxLength(20).IsRequired();
            entity.Property(e => e.IdFuncional).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Logradouro).HasMaxLength(200);
            entity.Property(e => e.Numero).HasMaxLength(20);
            entity.Property(e => e.CEP).HasMaxLength(10);
            entity.Property(e => e.Bairro).HasMaxLength(100);
            entity.Property(e => e.Cidade).HasMaxLength(100);

            entity.HasOne(e => e.ResponsavelFinanceiro)
                  .WithMany()
                  .HasForeignKey(e => e.ResponsavelFinanceiroId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== Usuario ==========
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Login).IsUnique();
            entity.Property(e => e.Login).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SenhaHash).HasMaxLength(500).IsRequired();

            entity.HasOne(e => e.Pessoa)
                  .WithOne(p => p.Usuario)
                  .HasForeignKey<Usuario>(e => e.PessoaId);
        });

        // ========== Curso ==========
        modelBuilder.Entity<Curso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IdFuncional).IsUnique();
            entity.Property(e => e.Nome).HasMaxLength(200).IsRequired();
            entity.Property(e => e.IdFuncional).HasMaxLength(10).IsRequired();
        });

        // ========== Disciplina ==========
        modelBuilder.Entity<Disciplina>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IdFuncional).IsUnique();
            entity.Property(e => e.Nome).HasMaxLength(200).IsRequired();
            entity.Property(e => e.IdFuncional).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Descricao).HasMaxLength(1000);
        });

        // ========== DisciplinaCurso (N:N) ==========
        modelBuilder.Entity<DisciplinaCurso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CursoId, e.DisciplinaId }).IsUnique();

            entity.HasOne(e => e.Curso)
                  .WithMany(c => c.DisciplinaCursos)
                  .HasForeignKey(e => e.CursoId);

            entity.HasOne(e => e.Disciplina)
                  .WithMany(d => d.DisciplinaCursos)
                  .HasForeignKey(e => e.DisciplinaId);
        });

        // ========== Turma ==========
        modelBuilder.Entity<Turma>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IdFuncional).IsUnique();
            entity.Property(e => e.Nome).HasMaxLength(200).IsRequired();
            entity.Property(e => e.IdFuncional).HasMaxLength(10).IsRequired();

            entity.HasOne(e => e.Curso)
                  .WithMany(c => c.Turmas)
                  .HasForeignKey(e => e.CursoId);
        });

        // ========== TurmaDisciplina ==========
        modelBuilder.Entity<TurmaDisciplina>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TurmaId, e.DisciplinaId }).IsUnique();
        });

        // ========== Matricula ==========
        modelBuilder.Entity<Matricula>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AlunoId, e.TurmaId }).IsUnique();

            entity.HasOne(e => e.Aluno)
                  .WithMany(p => p.Matriculas)
                  .HasForeignKey(e => e.AlunoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Turma)
                  .WithMany(t => t.Matriculas)
                  .HasForeignKey(e => e.TurmaId);
        });

        // ========== DiarioClasse ==========
        modelBuilder.Entity<DiarioClasse>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Docente)
                  .WithMany()
                  .HasForeignKey(e => e.DocenteId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Turma)
                  .WithMany(t => t.DiarioClasses)
                  .HasForeignKey(e => e.TurmaId);

            entity.HasOne(e => e.Disciplina)
                  .WithMany()
                  .HasForeignKey(e => e.DisciplinaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== Avaliacao ==========
        modelBuilder.Entity<Avaliacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Peso).HasPrecision(5, 2);

            entity.HasOne(e => e.Turma)
                  .WithMany()
                  .HasForeignKey(e => e.TurmaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Disciplina)
                  .WithMany()
                  .HasForeignKey(e => e.DisciplinaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== NotaAluno ==========
        modelBuilder.Entity<NotaAluno>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AlunoId, e.AvaliacaoId }).IsUnique();
            entity.Property(e => e.Nota).HasPrecision(5, 2);

            entity.HasOne(e => e.Aluno)
                  .WithMany(p => p.Notas)
                  .HasForeignKey(e => e.AlunoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Avaliacao)
                  .WithMany(a => a.Notas)
                  .HasForeignKey(e => e.AvaliacaoId);
        });

        // ========== PresencaAluno ==========
        modelBuilder.Entity<PresencaAluno>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AlunoId, e.DiarioClasseId }).IsUnique();

            entity.HasOne(e => e.Aluno)
                  .WithMany(p => p.Presencas)
                  .HasForeignKey(e => e.AlunoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DiarioClasse)
                  .WithMany(d => d.Presencas)
                  .HasForeignKey(e => e.DiarioClasseId);
        });

        // ========== Mensalidade ==========
        modelBuilder.Entity<Mensalidade>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Valor).HasPrecision(10, 2);

            entity.HasOne(e => e.Aluno)
                  .WithMany(p => p.Mensalidades)
                  .HasForeignKey(e => e.AlunoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== PagamentoEscola ==========
        modelBuilder.Entity<PagamentoEscola>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ValorPago).HasPrecision(10, 2);

            entity.HasOne(e => e.Mensalidade)
                  .WithOne(m => m.PagamentoEscola)
                  .HasForeignKey<PagamentoEscola>(e => e.MensalidadeId);

            entity.HasOne(e => e.Operador)
                  .WithMany()
                  .HasForeignKey(e => e.OperadorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== LancamentoFinanceiro ==========
        modelBuilder.Entity<LancamentoFinanceiro>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Descricao).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Valor).HasPrecision(10, 2);
        });

        // ========== CategoriaFinanceira ==========
        modelBuilder.Entity<CategoriaFinanceira>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).HasMaxLength(100).IsRequired();
        });

        // ========== CronogramaAula ==========
        modelBuilder.Entity<CronogramaAula>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Sala).HasMaxLength(50);

            entity.HasOne(e => e.Turma)
                  .WithMany(t => t.Cronogramas)
                  .HasForeignKey(e => e.TurmaId);

            entity.HasOne(e => e.Disciplina)
                  .WithMany()
                  .HasForeignKey(e => e.DisciplinaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Docente)
                  .WithMany()
                  .HasForeignKey(e => e.DocenteId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== ConfiguracaoEscolar ==========
        modelBuilder.Entity<ConfiguracaoEscolar>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MediaMinimaAprovacao).HasPrecision(5, 2);
            entity.Property(e => e.FrequenciaMinimaPercent).HasPrecision(5, 2);
        });

        // ========== TemplateDocumento ==========
        modelBuilder.Entity<TemplateDocumento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NomeArquivo).HasMaxLength(200);
            entity.Property(e => e.CaminhoArquivo).HasMaxLength(500);
        });

        // Seed: Configuração escolar padrão
        modelBuilder.Entity<ConfiguracaoEscolar>().HasData(new ConfiguracaoEscolar
        {
            Id = 1,
            MediaMinimaAprovacao = 7.0m,
            FrequenciaMinimaPercent = 75.0m,
            HorasAulaPadraoPorDia = 4,
            UltimaAtualizacao = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
