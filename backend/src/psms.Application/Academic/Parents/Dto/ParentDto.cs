using Abp.Application.Services.Dto;
using psms.Academic.Shared;
using System;

namespace psms.Academic.Parents.Dto;

/// <summary>
/// Full DTO for Parent entity including computed properties.
/// </summary>
public class ParentDto : FullAuditedEntityDto<Guid>
{
    public long UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string IdNumber { get; set; }
    public AddressDto Address { get; set; }
    public string Occupation { get; set; }
    public string Employer { get; set; }
    public string WorkPhone { get; set; }
    public string ProfilePhotoUrl { get; set; }

    /// <summary>Number of linked students</summary>
    public int StudentCount { get; set; }
}
