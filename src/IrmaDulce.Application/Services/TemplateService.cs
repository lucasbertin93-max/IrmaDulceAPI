using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.Domain.Entities;
using IrmaDulce.Domain.Interfaces;
using System.IO;

namespace IrmaDulce.Application.Services;

public class TemplateService : ITemplateService
{
    private readonly ITemplateDocumentoRepository _templateRepo;
    private readonly ITemplateTagRepository _tagRepo;

    public TemplateService(ITemplateDocumentoRepository templateRepo, ITemplateTagRepository tagRepo)
    {
        _templateRepo = templateRepo;
        _tagRepo = tagRepo;
    }

    public async Task<TemplateDocumentoResponse> UploadTemplateAsync(UploadTemplateRequest request, string webRootPath)
    {
        if (request.ArquivoBytes == null || request.ArquivoBytes.Length == 0)
            throw new ArgumentException("Nenhum arquivo enviado.");

        var ext = Path.GetExtension(request.NomeArquivo).ToLower();
        if (ext != ".docx")
            throw new ArgumentException("Apenas arquivos .docx são permitidos.");

        // Cria a pasta templates se não existir
        var templatesDir = Path.Combine(webRootPath, "templates");
        if (!Directory.Exists(templatesDir))
            Directory.CreateDirectory(templatesDir);

        var nomeArquivo = $"{request.TipoDocumento}_{DateTime.Now:yyyyMMddHHmmss}.docx";
        var caminhoAbsoluto = Path.Combine(templatesDir, nomeArquivo);

        await File.WriteAllBytesAsync(caminhoAbsoluto, request.ArquivoBytes);

        var caminhoRelativo = $"/templates/{nomeArquivo}";

        var template = await _templateRepo.GetByTipoAsync(request.TipoDocumento);

        if (template == null)
        {
            template = new TemplateDocumento
            {
                Tipo = request.TipoDocumento,
                NomeArquivo = request.NomeArquivo,
                CaminhoArquivo = caminhoRelativo,
                Ativo = true
            };
            await _templateRepo.AddAsync(template);
        }
        else
        {
            // Arquivo antigo pode ser deletado, mas por segurança só sobrescrevemos na base
            template.NomeArquivo = request.NomeArquivo;
            template.CaminhoArquivo = caminhoRelativo;
            await _templateRepo.UpdateAsync(template);
        }

        return await GetTemplateByTipoAsync(request.TipoDocumento) 
               ?? throw new Exception("Falha ao recuperar o template salvo.");
    }

    public async Task<TemplateDocumentoResponse?> GetTemplateByTipoAsync(Domain.Enums.TipoDocumento tipo)
    {
        var template = await _templateRepo.GetByTipoAsync(tipo);
        if (template == null) return null;

        return new TemplateDocumentoResponse
        {
            Id = template.Id,
            Tipo = template.Tipo,
            NomeArquivo = template.NomeArquivo,
            Ativo = template.Ativo,
            DataCadastro = template.DataCadastro,
            Tags = template.Tags.Select(t => new TemplateTagResponse
            {
                Id = t.Id,
                TagNoDocumento = t.TagNoDocumento,
                CampoSistema = t.CampoSistema
            }).ToList()
        };
    }

    public async Task<TemplateDocumentoResponse> SaveTagsAsync(SaveTagsRequest request)
    {
        var template = await _templateRepo.GetByTipoAsync(request.TipoDocumento);
        if (template == null)
            throw new KeyNotFoundException($"Template não encontrado para o tipo {request.TipoDocumento}. Faça o upload primeiro.");

        // Obter tags atuais
        var tagsAtuais = await _tagRepo.GetByTemplateIdAsync(template.Id);

        // Deletar todas as tags existentes
        foreach (var tag in tagsAtuais)
        {
            await _tagRepo.DeleteAsync(tag);
        }

        // Inserir as novas tags
        if (request.Tags != null && request.Tags.Any())
        {
            foreach (var reqTag in request.Tags)
            {
                var newTag = new TemplateTag
                {
                    TemplateDocumentoId = template.Id,
                    TagNoDocumento = reqTag.TagNoDocumento,
                    CampoSistema = reqTag.CampoSistema
                };
                await _tagRepo.AddAsync(newTag);
            }
        }

        return await GetTemplateByTipoAsync(request.TipoDocumento)
               ?? throw new Exception("Falha ao recuperar o template após salvar as tags.");
    }
}
