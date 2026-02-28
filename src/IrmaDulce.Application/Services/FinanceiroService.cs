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
        var alunos = await _pessoaRepo.FindAsync(p => p.Perfil == PerfilUsuario.Aluno && p.Ativo);

        foreach (var aluno in alunos)
        {
            var existentes = await _mensalidadeRepo.FindAsync(m =>
                m.AlunoId == aluno.Id &&
                m.MesReferencia == request.MesReferencia &&
                m.AnoReferencia == request.AnoReferencia);

            if (existentes.Any()) continue;

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

    public async Task GerarBoletosAlunoAsync(GerarBoletosAlunoRequest request)
    {
        var aluno = await _pessoaRepo.GetByIdAsync(request.AlunoId)
            ?? throw new KeyNotFoundException($"Aluno com ID {request.AlunoId} n\u00e3o encontrado.");

        for (int i = 0; i < request.QtdParcelas; i++)
        {
            var vencimento = request.PrimeiroVencimento.AddMonths(i);
            var mensalidade = new Mensalidade
            {
                AlunoId = request.AlunoId,
                MesReferencia = vencimento.Month,
                AnoReferencia = vencimento.Year,
                Valor = request.ValorParcela,
                DataVencimento = vencimento,
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

        // Atualiza status de atrasadas automaticamente
        var agora = DateTime.UtcNow;
        foreach (var m in mensalidades)
        {
            if (m.Status == StatusMensalidade.EmAberto && m.DataVencimento < agora)
            {
                m.Status = StatusMensalidade.Atrasado;
                await _mensalidadeRepo.UpdateAsync(m);
            }
        }

        if (status.HasValue)
            mensalidades = mensalidades.Where(m => m.Status == status.Value);
        if (mes.HasValue)
            mensalidades = mensalidades.Where(m => m.MesReferencia == mes.Value);
        if (ano.HasValue)
            mensalidades = mensalidades.Where(m => m.AnoReferencia == ano.Value);

        var results = new List<MensalidadeResponse>();
        foreach (var m in mensalidades)
            results.Add(await MapMensalidadeToResponseAsync(m));
        return results;
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

        return await MapMensalidadeToResponseAsync(m);
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

        // Entradas = mensalidades pagas (baseadas na data de pagamento) + lançamentos de entrada
        var totalMensalidadesPagas = mensalidades
            .Where(m => m.Status == StatusMensalidade.Pago && m.DataPagamento >= inicio && m.DataPagamento <= fim)
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

    private async Task<MensalidadeResponse> MapMensalidadeToResponseAsync(Mensalidade m)
    {
        // Load aluno if not loaded
        var aluno = m.Aluno ?? await _pessoaRepo.GetByIdAsync(m.AlunoId);
        Pessoa? responsavel = null;
        if (aluno?.ResponsavelFinanceiroId.HasValue == true)
            responsavel = await _pessoaRepo.GetByIdAsync(aluno.ResponsavelFinanceiroId.Value);

        var endereco = aluno != null
            ? $"{aluno.Logradouro}, {aluno.Numero} - {aluno.Bairro}, {aluno.Cidade} - CEP {aluno.CEP}"
            : "";

        // Get turma from active enrollment
        string? turmaNome = null;
        if (aluno != null)
        {
            var mats = await _matriculaRepo.GetByAlunoIdAsync(aluno.Id);
            var ativa = mats.FirstOrDefault(mt => mt.Status == StatusMatricula.Ativo);
            turmaNome = ativa?.Turma?.Nome;
        }

        // Number parcela: count by aluno ordered by date
        var todasDoAluno = (await _mensalidadeRepo.GetByAlunoIdAsync(m.AlunoId))
            .OrderBy(x => x.DataVencimento).ToList();
        var numParcela = todasDoAluno.FindIndex(x => x.Id == m.Id) + 1;
        var totalParcelas = todasDoAluno.Count;

        return new MensalidadeResponse(
            Id: m.Id,
            AlunoId: m.AlunoId,
            AlunoNome: aluno?.NomeCompleto ?? "",
            AlunoIdFuncional: aluno?.IdFuncional ?? "",
            MesReferencia: m.MesReferencia,
            AnoReferencia: m.AnoReferencia,
            Valor: m.Valor,
            DataVencimento: m.DataVencimento,
            DataPagamento: m.DataPagamento,
            Status: m.Status,
            ResponsavelNome: responsavel?.NomeCompleto,
            EnderecoCompleto: endereco,
            TurmaNome: turmaNome,
            NumeroParcela: numParcela,
            TotalParcelas: totalParcelas
        );
    }
}
