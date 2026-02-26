using IrmaDulce.Domain.Entities;
using IrmaDulce.Domain.Enums;

namespace IrmaDulce.Application.DTOs;

// ==================== Auth ====================
public record LoginRequest(string Login, string Senha);
public record LoginResponse(string Token, string RefreshToken, string Nome, string Perfil, string IdFuncional);

// ==================== Pessoa ====================
public record PessoaCreateRequest(
    string NomeCompleto,
    string RG,
    string CPF,
    EstadoCivil EstadoCivil,
    DateTime DataNascimento,
    string Naturalidade,
    string Nacionalidade,
    string Logradouro,
    string Numero,
    string CEP,
    string Bairro,
    string Cidade,
    string? PontoReferencia,
    string NomePai,
    string NomeMae,
    PerfilUsuario Perfil,
    int? ResponsavelFinanceiroId,
    // Dados do responsável financeiro (quando não é o aluno)
    PessoaCreateRequest? ResponsavelFinanceiro
);

public record PessoaResponse(
    int Id,
    string IdFuncional,
    string NomeCompleto,
    string CPF,
    string RG,
    EstadoCivil EstadoCivil,
    DateTime DataNascimento,
    string Naturalidade,
    string Nacionalidade,
    string Logradouro,
    string Numero,
    string CEP,
    string Bairro,
    string Cidade,
    string? PontoReferencia,
    string NomePai,
    string NomeMae,
    PerfilUsuario Perfil,
    bool Ativo,
    int? ResponsavelFinanceiroId
);

// ==================== Curso ====================
public record CursoRequest(string Nome, int CargaHoraria);
public record CursoResponse(int Id, string IdFuncional, string Nome, int CargaHoraria, bool Ativo);

// ==================== Disciplina ====================
public record DisciplinaRequest(string Nome, int CargaHoraria, string? Descricao);
public record DisciplinaResponse(int Id, string IdFuncional, string Nome, int CargaHoraria, string? Descricao, bool Ativo);

// ==================== Turma ====================
public record TurmaCreateRequest(string Nome, string? Horario, DateTime DataInicio, DateTime DataFim, int CursoId);
public record TurmaResponse(int Id, string IdFuncional, string Nome, string? Horario, DateTime DataInicio, DateTime DataFim, int CursoId, string CursoNome, bool Ativo);

// ==================== Matrícula ====================
public record MatriculaRequest(int AlunoId, int TurmaId);
public record MatriculaResponse(int Id, int AlunoId, string AlunoNome, string AlunoIdFuncional, int TurmaId, string TurmaNome, StatusMatricula Status, DateTime DataMatricula);

// ==================== Diário de Classe ====================
public record DiarioClasseRequest(
    DateTime Data,
    int DocenteId,
    int TurmaId,
    int DisciplinaId,
    int QuantidadeHorasAula,
    string? ConteudoMinistrado,
    string? Observacoes,
    List<PresencaRequest>? Presencas
);

public record PresencaRequest(int AlunoId, bool Presente, bool FaltaJustificada, string? Justificativa);

// ==================== Avaliação ====================
public record AvaliacaoRequest(string Nome, string? Descricao, DateTime? DataAplicacao, decimal Peso, int TurmaId, int DisciplinaId);
public record AvaliacaoResponse(int Id, string Nome, string? Descricao, DateTime? DataAplicacao, decimal Peso, int TurmaId, int DisciplinaId);

// ==================== Nota ====================
public record NotaRequest(int AlunoId, int AvaliacaoId, decimal Nota, string? Observacao);

// ==================== Mensalidade ====================
public record GerarMensalidadesRequest(int MesReferencia, int AnoReferencia, decimal Valor, DateTime DataVencimento);
public record GerarBoletosAlunoRequest(int AlunoId, int QtdParcelas, decimal ValorParcela, DateTime PrimeiroVencimento);
public record RegistrarPagamentoRequest(int MensalidadeId, decimal ValorPago, MetodoPagamento MetodoPagamento, DateTime DataPagamento, string? Observacao);
public record MensalidadeResponse(
    int Id, int AlunoId, string AlunoNome, string AlunoIdFuncional,
    int MesReferencia, int AnoReferencia, decimal Valor,
    DateTime DataVencimento, DateTime? DataPagamento, StatusMensalidade Status,
    string? ResponsavelNome, string? EnderecoCompleto, string? TurmaNome,
    int NumeroParcela, int TotalParcelas);

// ==================== Financeiro ====================
public record LancamentoRequest(DateTime Data, string Descricao, decimal Valor, TipoLancamento Tipo, int? CategoriaId);
public record LancamentoResponse(int Id, DateTime Data, string Descricao, decimal Valor, TipoLancamento Tipo, string? CategoriaNome);
public record DashboardFinanceiroResponse(decimal TotalEntradas, decimal TotalSaidas, decimal Saldo, int MensalidadesAReceber, int MensalidadesRecebidas, int MensalidadesAtrasadas);

// ==================== Cronograma ====================
public record CronogramaRequest(int TurmaId, int DisciplinaId, int DocenteId, DateTime Data, TimeSpan HoraInicio, TimeSpan HoraFim, string? Sala);
public record CronogramaResponse(int Id, int TurmaId, string TurmaNome, int DisciplinaId, string DisciplinaNome, int DocenteId, string DocenteNome, DateTime Data, TimeSpan HoraInicio, TimeSpan HoraFim, string? Sala);
public record ConflitoCronogramaResponse(string Tipo, string Mensagem);

// ==================== Configuração ====================
public record ConfiguracaoRequest(decimal MediaMinimaAprovacao, decimal FrequenciaMinimaPercent, int HorasAulaPadraoPorDia);
public record ConfiguracaoResponse(decimal MediaMinimaAprovacao, decimal FrequenciaMinimaPercent, int HorasAulaPadraoPorDia);

// ==================== Documento ====================
public record EmitirDocumentoRequest(int AlunoId, TipoDocumento TipoDocumento, string? SenhaMasterOverride);
