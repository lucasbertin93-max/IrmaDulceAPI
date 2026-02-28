using IrmaDulce.Domain.Enums;

namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Mapeia uma tag de texto num template (ex: {{NOME_ALUNO}}) para um campo do sistema.
/// </summary>
public class TemplateTag
{
    public int Id { get; set; }
    
    // Relacionamento com TemplateDocumento
    public int TemplateDocumentoId { get; set; }
    public TemplateDocumento TemplateDocumento { get; set; } = null!;

    // A tag exata no arquivo .docx, ex: "{{NOME_ALUNO}}"
    public string TagNoDocumento { get; set; } = string.Empty;

    // O campo do sistema ao qual esta tag se refere, ex: "Pessoa.NomeCompleto"
    public string CampoSistema { get; set; } = string.Empty;
}
