using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.Domain.Interfaces;

namespace IrmaDulce.Application.Services;

/// <summary>
/// Serviço de emissão de documentos com verificação de bloqueio financeiro.
/// Regras de negócio: 7.1, 7.2, 7.3
/// </summary>
public class DocumentoService : IDocumentoService
{
    private readonly IMensalidadeRepository _mensalidadeRepo;
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly ITemplateDocumentoRepository _templateRepo;
    private readonly IPessoaRepository _pessoaRepo;
    private readonly IMatriculaRepository _matriculaRepo;
    private readonly ICursoRepository _cursoRepo;
    private readonly ITurmaRepository _turmaRepo;

    public DocumentoService(
        IMensalidadeRepository mensalidadeRepo,
        IUsuarioRepository usuarioRepo,
        ITemplateDocumentoRepository templateRepo,
        IPessoaRepository pessoaRepo,
        IMatriculaRepository matriculaRepo,
        ICursoRepository cursoRepo,
        ITurmaRepository turmaRepo)
    {
        _mensalidadeRepo = mensalidadeRepo;
        _usuarioRepo = usuarioRepo;
        _templateRepo = templateRepo;
        _pessoaRepo = pessoaRepo;
        _matriculaRepo = matriculaRepo;
        _cursoRepo = cursoRepo;
        _turmaRepo = turmaRepo;
    }

    public async Task<byte[]> EmitirDocumentoAsync(EmitirDocumentoRequest request, int operadorId)
    {
        var aluno = await _pessoaRepo.GetByIdWithResponsavelAsync(request.AlunoId)
            ?? throw new KeyNotFoundException($"Aluno com ID {request.AlunoId} não encontrado.");

        // Verifica bloqueio financeiro (regra 7.3)
        var inadimplente = await _mensalidadeRepo.AlunoInadimplente(request.AlunoId);

        if (inadimplente)
        {
            // Se Master Override fornecido, valida a senha
            if (!string.IsNullOrEmpty(request.SenhaMasterOverride))
            {
                // Busca o operador pelo ID para verificar se é Master
                var operadorUsuario = await _usuarioRepo.GetByIdAsync(operadorId);
                if (operadorUsuario == null || operadorUsuario.Perfil != Domain.Enums.PerfilUsuario.Master)
                    throw new UnauthorizedAccessException("Apenas o perfil Master pode autorizar emissão com bloqueio financeiro.");

                if (!BCrypt.Net.BCrypt.Verify(request.SenhaMasterOverride, operadorUsuario.SenhaHash))
                    throw new UnauthorizedAccessException("Senha de autorização inválida.");

                // Log de auditoria — senha válida, continua a emissão
            }
            else
            {
                throw new InvalidOperationException("BLOQUEIO_FINANCEIRO: Aluno possui mensalidades em atraso. Informe a senha do Master para prosseguir.");
            }
        }

        // Busca template do documento
        var template = await _templateRepo.GetByTipoAsync(request.TipoDocumento);

        // Busca dados para preenchimento dos placeholders
        var matriculas = await _matriculaRepo.GetByAlunoIdAsync(request.AlunoId);
        var matriculaAtiva = matriculas.FirstOrDefault(m => m.Status == Domain.Enums.StatusMatricula.Ativo);
        Domain.Entities.Turma? turma = null;
        Domain.Entities.Curso? curso = null;

        if (matriculaAtiva != null)
        {
            turma = await _turmaRepo.GetByIdAsync(matriculaAtiva.TurmaId);
            if (turma != null)
                curso = await _cursoRepo.GetByIdAsync(turma.CursoId);
        }

        // Monta dicionário de placeholders (regra 7.1)
        var placeholders = new Dictionary<string, string>
        {
            { "{{NOME_ALUNO}}", aluno.NomeCompleto },
            { "{{CPF_ALUNO}}", aluno.CPF },
            { "{{RG_ALUNO}}", aluno.RG },
            { "{{ID_ALUNO}}", aluno.IdFuncional },
            { "{{CURSO_NOME}}", curso?.Nome ?? "N/A" },
            { "{{TURMA_NOME}}", turma?.Nome ?? "N/A" },
            { "{{DATA_INICIO}}", turma?.DataInicio.ToString("dd/MM/yyyy") ?? "N/A" },
            { "{{DATA_FIM}}", turma?.DataFim.ToString("dd/MM/yyyy") ?? "N/A" },
            { "{{CARGA_HORARIA}}", curso?.CargaHoraria.ToString() ?? "N/A" },
            { "{{DATA_EMISSAO}}", DateTime.Now.ToString("dd/MM/yyyy") },
        };

        var responsavel = aluno.ResponsavelFinanceiro ?? aluno;
        
        var hoje = DateTime.Now;
        var idadeAluno = hoje.Year - aluno.DataNascimento.Year;
        if (aluno.DataNascimento.Date > hoje.AddYears(-idadeAluno)) idadeAluno--;
        
        var idadeResp = hoje.Year - responsavel.DataNascimento.Year;
        if (responsavel.DataNascimento.Date > hoje.AddYears(-idadeResp)) idadeResp--;

        var valoresSistema = new Dictionary<string, string>
        {
            // Sistema
            { "Sistema.DataEmissao", DateTime.Now.ToString("dd/MM/yyyy") },
            // Curso & Turma
            { "Curso.Nome", curso?.Nome ?? "" },
            { "Curso.CargaHoraria", curso?.CargaHoraria.ToString() ?? "" },
            { "Turma.Nome", turma?.Nome ?? "" },
            { "Turma.DataInicio", turma?.DataInicio.ToString("dd/MM/yyyy") ?? "" },
            { "Turma.DataFim", turma?.DataFim.ToString("dd/MM/yyyy") ?? "" },
            // Aluno
            { "Aluno.NomeCompleto", aluno.NomeCompleto },
            { "Aluno.Idade", idadeAluno.ToString() },
            { "Aluno.DataNascimento", aluno.DataNascimento.ToString("dd/MM/yyyy") },
            { "Aluno.Naturalidade", aluno.Naturalidade },
            { "Aluno.Nacionalidade", aluno.Nacionalidade },
            { "Aluno.Sexo", aluno.Sexo.ToString() },
            { "Aluno.EstadoCivil", aluno.EstadoCivil.ToString() },
            { "Aluno.RG", aluno.RG },
            { "Aluno.CPF", aluno.CPF },
            { "Aluno.Logradouro", aluno.Logradouro },
            { "Aluno.Numero", aluno.Numero },
            { "Aluno.Bairro", aluno.Bairro },
            { "Aluno.CEP", aluno.CEP },
            { "Aluno.Cidade", aluno.Cidade },
            { "Aluno.UF", aluno.Cidade.Length >= 2 && aluno.Cidade.Contains("-") ? aluno.Cidade.Split('-').Last().Trim().ToUpper() : "" },
            { "Aluno.Telefone", aluno.Telefone },
            { "Aluno.Email", aluno.Email },
            { "Aluno.PontoReferencia", aluno.PontoReferencia ?? "" },
            // Responsável/Contratante
            { "Responsavel.NomeCompleto", responsavel.NomeCompleto },
            { "Responsavel.Idade", idadeResp.ToString() },
            { "Responsavel.DataNascimento", responsavel.DataNascimento.ToString("dd/MM/yyyy") },
            { "Responsavel.Naturalidade", responsavel.Naturalidade },
            { "Responsavel.Nacionalidade", responsavel.Nacionalidade },
            { "Responsavel.Sexo", responsavel.Sexo.ToString() },
            { "Responsavel.EstadoCivil", responsavel.EstadoCivil.ToString() },
            { "Responsavel.RG", responsavel.RG },
            { "Responsavel.CPF", responsavel.CPF },
            { "Responsavel.Logradouro", responsavel.Logradouro },
            { "Responsavel.Numero", responsavel.Numero },
            { "Responsavel.Bairro", responsavel.Bairro },
            { "Responsavel.CEP", responsavel.CEP },
            { "Responsavel.Cidade", responsavel.Cidade },
            { "Responsavel.UF", responsavel.Cidade.Length >= 2 && responsavel.Cidade.Contains("-") ? responsavel.Cidade.Split('-').Last().Trim().ToUpper() : "" },
            { "Responsavel.Telefone", responsavel.Telefone },
            { "Responsavel.Email", responsavel.Email },
            { "Responsavel.PontoReferencia", responsavel.PontoReferencia ?? "" },
            // Deprecated 
            { "Pessoa.NomeCompleto", aluno.NomeCompleto },
            { "Pessoa.CPF", aluno.CPF },
            { "Pessoa.RG", aluno.RG },
            { "Pessoa.IdFuncional", aluno.IdFuncional }
        };

        // Se template existe, processa o arquivo DOCX
        if (template != null && !string.IsNullOrEmpty(template.CaminhoArquivo))
        {
            var webRootPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot");
            var absolutePath = System.IO.Path.Combine(webRootPath, template.CaminhoArquivo.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(absolutePath))
            {
                using var memoryStream = new System.IO.MemoryStream();
                using (var doc = Xceed.Words.NET.DocX.Load(absolutePath))
                {
                    // Fallback para tags antigas caso estejam em uso no template
                    foreach(var kvp in placeholders)
                    {
                        doc.ReplaceText(kvp.Key, kvp.Value);
                    }
                    
                    // Substitui dinâmicas mapeadas
                    if (template.Tags != null)
                    {
                        foreach(var tag in template.Tags)
                        {
                            var valor = valoresSistema.GetValueOrDefault(tag.CampoSistema, "");
                            doc.ReplaceText(tag.TagNoDocumento, valor);
                        }
                    }
                    
                    doc.SaveAs(memoryStream);
                }
                return memoryStream.ToArray();
            }
        }

        // Se não há template, gera um documento simples
        return GeneratePlaceholderPdf(aluno.NomeCompleto, request.TipoDocumento.ToString(), placeholders);
    }

    private static byte[] GeneratePlaceholderPdf(string alunoNome, string tipoDocumento, Dictionary<string, string> dados)
    {
        // Gera um texto simples como placeholder até implementar template engine
        var conteudo = $"""
            ESCOLA DE ENFERMAGEM IRMA DULCE
            ================================
            
            Tipo de Documento: {tipoDocumento}
            
            Aluno: {dados.GetValueOrDefault("{{NOME_ALUNO}}", "")}
            CPF: {dados.GetValueOrDefault("{{CPF_ALUNO}}", "")}
            RG: {dados.GetValueOrDefault("{{RG_ALUNO}}", "")}
            ID Funcional: {dados.GetValueOrDefault("{{ID_ALUNO}}", "")}
            
            Curso: {dados.GetValueOrDefault("{{CURSO_NOME}}", "")}
            Turma: {dados.GetValueOrDefault("{{TURMA_NOME}}", "")}
            Período: {dados.GetValueOrDefault("{{DATA_INICIO}}", "")} a {dados.GetValueOrDefault("{{DATA_FIM}}", "")}
            Carga Horária: {dados.GetValueOrDefault("{{CARGA_HORARIA}}", "")} horas
            
            Data de Emissão: {dados.GetValueOrDefault("{{DATA_EMISSAO}}", "")}
            """;

        return System.Text.Encoding.UTF8.GetBytes(conteudo);
    }
}
