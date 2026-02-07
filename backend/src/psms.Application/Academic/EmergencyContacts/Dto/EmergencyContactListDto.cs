using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Academic.EmergencyContacts.Dto;

public class EmergencyContactListDto : EntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public RelationshipType Relationship { get; set; }
    public string PrimaryPhone { get; set; }
    public int Priority { get; set; }
    public bool CanPickUp { get; set; }
    public bool IsActive { get; set; }
}
