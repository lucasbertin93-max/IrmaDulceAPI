using IrmaDulce.Application.DTOs;
using IrmaDulce.Domain.Enums;

namespace IrmaDulce.Application.Interfaces;

public interface ITemplateService
{
    Task<TemplateDocumentoResponse> UploadTemplateAsync(UploadTemplateRequest request, string webRootPath);
    Task<TemplateDocumentoResponse?> GetTemplateByTipoAsync(TipoDocumento tipo);
    Task<TemplateDocumentoResponse> SaveTagsAsync(SaveTagsRequest request);
}
