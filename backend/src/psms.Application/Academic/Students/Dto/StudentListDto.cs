using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Academic.Students.Dto;

/// <summary>
/// Lightweight list DTO for Student entity.
/// </summary>
public class StudentListDto : EntityDto<Guid>
{
    public string FullName { get; set; }
    public string AdmissionNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public Guid CurrentGradeId { get; set; }
    public string CurrentGradeName { get; set; }
    public Guid CurrentClassId { get; set; }
    public string CurrentClassName { get; set; }
    public bool IsActive { get; set; }
    public int ParentCount { get; set; }
}
