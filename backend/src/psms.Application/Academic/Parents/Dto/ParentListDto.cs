using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.Parents.Dto;

/// <summary>
/// Lightweight DTO for parent lists and dropdowns.
/// </summary>
public class ParentListDto : EntityDto<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public int StudentCount { get; set; }
}
