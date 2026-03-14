using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Communication.Documents.Dto;

public class GetDocumentsInput : PagedAndSortedResultRequestDto
{
    public SharedDocumentType? DocumentType { get; set; }
    public DocumentAudience? TargetAudience { get; set; }
    public string Category { get; set; }
    public bool? IsPublished { get; set; }
    public Guid? AcademicYearId { get; set; }
    public string Search { get; set; }
    public string Keyword { get; set; }
}
