using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.Domain.Entities;
using IrmaDulce.Domain.Enums;
using IrmaDulce.Domain.Interfaces;

namespace IrmaDulce.Application.Services;

public class FinanceiroService : IFinanceiroService
{
    private readonly IMensalidadeRepository _mensalidadeRepo;
    private readonly IPagamentoEscolaRepository _pagamentoRepo;
    private readonly ILancamentoFinanceiroRepository _lancamentoRepo;
    private readonly IMatriculaRepository _matriculaRepo;
    private readonly IPessoaRepository _pessoaRepo;

    public FinanceiroService(
        IMensalidadeRepository mensalidadeRepo,
        IPagamentoEscolaRepository pagamentoRepo,
        ILancamentoFinanceiroRepository lancamentoRepo,
        IMatriculaRepository matriculaRepo,
        IPessoaRepository pessoaRepo)
    {
        _mensalidadeRepo = mensalidadeRepo;
        _pagamentoRepo = pagamentoRepo;
        _lancamentoRepo = lancamentoRepo;
        _matriculaRepo = matriculaRepo;
        _pessoaRepo = pessoaRepo;
    }

    /// <summary>
    /// Gera mensalidades em massa para todos os alunos ativos.
    /// Regra de negócio 8.1
    /// </summary>
    public async Task GerarMensalidadesAsync(GerarMensalidadesRequest request)
    {
        // Busca todos os alunos ativos (com matrícula ativa)
        var alunos = await _pessoaRepo.FindAsync(p => p.Perfil == PerfilUsuario.Aluno && p.Ativo);

        foreach (var aluno in alunos)
        {
            // Verifica se já existe mensalidade p/ este aluno, mês e ano
            var existentes = await _mensalidadeRepo.FindAsync(m =>
                m.AlunoId == aluno.Id &&
                m.MesReferencia == request.MesReferencia &&
                m.AnoReferencia == request.AnoReferencia);

            if (existentes.Any()) continue; // Não duplicar

            var mensalidade = new Mensalidade
            {
                AlunoId = aluno.Id,
                MesReferencia = request.MesReferencia,
                AnoReferencia = request.AnoReferencia,
                Valor = request.Valor,
                DataVencimento = request.DataVencimento,
                Status = StatusMensalidade.EmAberto,
            };

            await _mensalidadeRepo.AddAsync(mensalidade);
        }
    }

    public async Task<IEnumerable<MensalidadeResponse>> GetMensalidadesAsync(
        int? alunoId, StatusMensalidade? status, int? mes, int? ano)
    {
        IEnumerable<Mensalidade> mensalidades;

        if (alunoId.HasValue)
            mensalidades = await _mensalidadeRepo.GetByAlunoIdAsync(alunoId.Value);
        else
            mensalidades = await _mensalidadeRepo.GetAllAsync();

        // Atualiza status de atrasadas automaticamente (regra 8.1)
        var agora = DateTime.UtcNow;
        foreach (var m in mensalidades)
        {
            if (m.Status == StatusMensalidade.EmAberto && m.DataVencimento < agora)
            {
                m.Status = StatusMensalidade.Atrasado;
                await _mensalidadeRepo.UpdateAsync(m);
            }
        }

        // Filtra
        if (status.HasValue)
            mensalidades = mensalidades.Where(m => m.Status == status.Value);
        if (mes.HasValue)
            mensalidades = mensalidades.Where(m => m.MesReferencia == mes.Value);
        if (ano.HasValue)
            mensalidades = mensalidades.Where(m => m.AnoReferencia == ano.Value);

        return mensalidades.Select(MapMensalidadeToResponse);
    }

    /// <summary>
    /// Registra pagamento presencial com dados do operador.
    /// Regra de negócio 8.2
    /// </summary>
    public async Task RegistrarPagamentoAsync(RegistrarPagamentoRequest request, int operadorId)
    {
        var mensalidade = await _mensalidadeRepo.GetByIdAsync(request.MensalidadeId)
            ?? throw new KeyNotFoundException($"Mensalidade com ID {request.MensalidadeId} não encontrada.");

        if (mensalidade.Status == StatusMensalidade.Pago)
            throw new InvalidOperationException("Esta mensalidade já foi paga.");

        // Atualiza a mensalidade
        mensalidade.Status = StatusMensalidade.Pago;
        mensalidade.DataPagamento = request.DataPagamento;
        await _mensalidadeRepo.UpdateAsync(mensalidade);

        // Registra o pagamento
        var pagamento = new PagamentoEscola
        {
            MensalidadeId = mensalidade.Id,
            ValorPago = request.ValorPago,
            MetodoPagamento = request.MetodoPagamento,
            DataPagamento = request.DataPagamento,
            OperadorId = operadorId,
            Observacao = request.Observacao,
        };

        await _pagamentoRepo.AddAsync(pagamento);
    }

    public async Task<MensalidadeResponse> AtualizarMensalidadeAsync(int id, decimal valor, DateTime dataVencimento)
    {
        var m = await _mensalidadeRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Mensalidade com ID {id} não encontrada.");

        m.Valor = valor;
        m.DataVencimento = dataVencimento;
        await _mensalidadeRepo.UpdateAsync(m);

        return MapMensalidadeToResponse(m);
    }

    public async Task DeletarMensalidadeAsync(int id)
    {
        var m = await _mensalidadeRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Mensalidade com ID {id} não encontrada.");

        if (m.Status == StatusMensalidade.Pago)
            throw new InvalidOperationException("Não é possível excluir uma mensalidade já paga.");

        m.Status = StatusMensalidade.Cancelado;
        await _mensalidadeRepo.UpdateAsync(m);
    }

    public async Task<LancamentoResponse> AdicionarLancamentoAsync(LancamentoRequest request)
    {
        var lancamento = new LancamentoFinanceiro
        {
            Data = request.Data,
            Descricao = request.Descricao,
            Valor = request.Valor,
            Tipo = request.Tipo,
            CategoriaId = request.CategoriaId,
        };

        await _lancamentoRepo.AddAsync(lancamento);

        return new LancamentoResponse(
            Id: lancamento.Id,
            Data: lancamento.Data,
            Descricao: lancamento.Descricao,
            Valor: lancamento.Valor,
            Tipo: lancamento.Tipo,
            CategoriaNome: null
        );
    }

    public async Task<IEnumerable<LancamentoResponse>> GetLancamentosAsync(
        DateTime inicio, DateTime fim, TipoLancamento? tipo)
    {
        var lancamentos = await _lancamentoRepo.GetByPeriodoAsync(inicio, fim);

        if (tipo.HasValue)
            lancamentos = lancamentos.Where(l => l.Tipo == tipo.Value);

        return lancamentos.Select(l => new LancamentoResponse(
            Id: l.Id,
            Data: l.Data,
            Descricao: l.Descricao,
            Valor: l.Valor,
            Tipo: l.Tipo,
            CategoriaNome: l.Categoria?.Nome
        ));
    }

    /// <summary>
    /// Dashboard financeiro com totais calculados.
    /// Regra de negócio 8.4
    /// </summary>
    public async Task<DashboardFinanceiroResponse> GetDashboardAsync(DateTime inicio, DateTime fim)
    {
        var lancamentos = await _lancamentoRepo.GetByPeriodoAsync(inicio, fim);
        var mensalidades = await _mensalidadeRepo.GetAllAsync();

        // Filtra mensalidades pelo período
        var mensalidadesNoPeriodo = mensalidades.Where(m =>
            m.DataVencimento >= inicio && m.DataVencimento <= fim);

        // Entradas = mensalidades pagas + lançamentos de entrada
        var totalMensalidadesPagas = mensalidadesNoPeriodo
            .Where(m => m.Status == StatusMensalidade.Pago)
            .Sum(m => m.Valor);

        var totalEntradasAvulsas = lancamentos
            .Where(l => l.Tipo == TipoLancamento.Entrada)
            .Sum(l => l.Valor);

        var totalEntradas = totalMensalidadesPagas + totalEntradasAvulsas;

        // Saídas = lançamentos de saída
        var totalSaidas = lancamentos
            .Where(l => l.Tipo == TipoLancamento.Saida)
            .Sum(l => l.Valor);

        // Contadores de mensalidades
        var aReceber = mensalidadesNoPeriodo.Count(m => m.Status == StatusMensalidade.EmAberto);
        var recebidas = mensalidadesNoPeriodo.Count(m => m.Status == StatusMensalidade.Pago);
        var atrasadas = mensalidadesNoPeriodo.Count(m => m.Status == StatusMensalidade.Atrasado);

        return new DashboardFinanceiroResponse(
            TotalEntradas: totalEntradas,
            TotalSaidas: totalSaidas,
            Saldo: totalEntradas - totalSaidas,
            MensalidadesAReceber: aReceber,
            MensalidadesRecebidas: recebidas,
            MensalidadesAtrasadas: atrasadas
        );
    }

    private static MensalidadeResponse MapMensalidadeToResponse(Mensalidade m) => new(
        Id: m.Id,
        AlunoId: m.AlunoId,
        AlunoNome: m.Aluno?.NomeCompleto ?? "",
        AlunoIdFuncional: m.Aluno?.IdFuncional ?? "",
        MesReferencia: m.MesReferencia,
        AnoReferencia: m.AnoReferencia,
        Valor: m.Valor,
        DataVencimento: m.DataVencimento,
        DataPagamento: m.DataPagamento,
        Status: m.Status
    );
}
