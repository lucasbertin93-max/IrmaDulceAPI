using IrmaDulce.Domain.Enums;
using IrmaDulce.Domain.Enums;

namespace IrmaDulce.Application.DTOs;

public class UploadTemplateRequest
{
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public TipoDocumento TipoDocumento { get; set; }
    public byte[] ArquivoBytes { get; set; } = Array.Empty<byte>();
    public string NomeArquivo { get; set; } = string.Empty;
}

public class TemplateTagRequest
{
    public string TagNoDocumento { get; set; } = string.Empty;
    public string CampoSistema { get; set; } = string.Empty;
}

public class SaveTagsRequest
{
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    public TipoDocumento TipoDocumento { get; set; }
    public List<TemplateTagRequest> Tags { get; set; } = new();
}

public class TemplateTagResponse
{
    public int Id { get; set; }
    public string TagNoDocumento { get; set; } = string.Empty;
    public string CampoSistema { get; set; } = string.Empty;
}

public class TemplateDocumentoResponse
{
    public int Id { get; set; }
    public TipoDocumento Tipo { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
    public List<TemplateTagResponse> Tags { get; set; } = new();
}
