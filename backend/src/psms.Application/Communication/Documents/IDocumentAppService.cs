using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Communication.Documents.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Communication.Documents;

public interface IDocumentAppService : IApplicationService
{
    Task<DocumentDto> GetAsync(Guid id);
    Task<PagedResultDto<DocumentListDto>> GetAllAsync(GetDocumentsInput input);
    Task<DocumentDto> CreateAsync(CreateDocumentDto input);
    Task<DocumentDto> UpdateAsync(Guid id, UpdateDocumentDto input);
    Task DeleteAsync(Guid id);
    Task<DocumentDto> PublishAsync(Guid id);
    Task<DocumentDto> UnpublishAsync(Guid id);
    Task<DocumentDto> RecordDownloadAsync(Guid id);
}
