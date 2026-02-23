using IrmaDulce.Domain.Enums;

namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Referência ao template .docx para geração de documentos.
/// </summary>
public class TemplateDocumento
{
    public int Id { get; set; }
    public TipoDocumento Tipo { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string CaminhoArquivo { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
}
