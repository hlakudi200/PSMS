using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Communication.Documents.Dto;

public class DocumentListDto : EntityDto<Guid>
{
    public string Title { get; set; }
    public string FileName { get; set; }
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; }
    public string Category { get; set; }
    public SharedDocumentType DocumentType { get; set; }
    public DocumentAudience TargetAudience { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedDate { get; set; }
    public long UploadedByUserId { get; set; }
    public int DownloadCount { get; set; }
}
