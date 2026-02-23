using IrmaDulce.Application.Interfaces;
using IrmaDulce.Application.Services;
using IrmaDulce.Domain.Interfaces;
using IrmaDulce.Infrastructure.Data;
using IrmaDulce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IrmaDulce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database (SQLite para desenvolvimento — será migrado para SQL cloud em produção)
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IPessoaRepository, PessoaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ICursoRepository, CursoRepository>();
        services.AddScoped<IDisciplinaRepository, DisciplinaRepository>();
        services.AddScoped<ITurmaRepository, TurmaRepository>();
        services.AddScoped<IMatriculaRepository, MatriculaRepository>();
        services.AddScoped<IDiarioClasseRepository, DiarioClasseRepository>();
        services.AddScoped<IAvaliacaoRepository, AvaliacaoRepository>();
        services.AddScoped<INotaAlunoRepository, NotaAlunoRepository>();
        services.AddScoped<IPresencaAlunoRepository, PresencaAlunoRepository>();
        services.AddScoped<IMensalidadeRepository, MensalidadeRepository>();
        services.AddScoped<IPagamentoEscolaRepository, PagamentoEscolaRepository>();
        services.AddScoped<ILancamentoFinanceiroRepository, LancamentoFinanceiroRepository>();
        services.AddScoped<ICategoriaFinanceiraRepository, CategoriaFinanceiraRepository>();
        services.AddScoped<ICronogramaAulaRepository, CronogramaAulaRepository>();
        services.AddScoped<IConfiguracaoEscolarRepository, ConfiguracaoEscolarRepository>();
        services.AddScoped<ITemplateDocumentoRepository, TemplateDocumentoRepository>();

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPessoaService, PessoaService>();
        services.AddScoped<ICursoService, CursoService>();
        services.AddScoped<IDisciplinaService, DisciplinaService>();
        services.AddScoped<ITurmaService, TurmaService>();
        services.AddScoped<IDiarioClasseService, DiarioClasseService>();
        services.AddScoped<IAvaliacaoService, AvaliacaoService>();
        services.AddScoped<IDocumentoService, DocumentoService>();
        services.AddScoped<IFinanceiroService, FinanceiroService>();
        services.AddScoped<ICronogramaService, CronogramaService>();
        services.AddScoped<IConfiguracaoService, ConfiguracaoService>();

        return services;
    }
}
