using psms.Domain.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.EmergencyContacts.Dto;

public class UpdateEmergencyContactDto
{
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; }

    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; }

    public RelationshipType? Relationship { get; set; }

    [StringLength(20)]
    public string PrimaryPhone { get; set; }

    [StringLength(20)]
    public string SecondaryPhone { get; set; }

    [StringLength(20)]
    public string WorkPhone { get; set; }

    [StringLength(256)]
    [EmailAddress]
    public string Email { get; set; }

    [StringLength(500)]
    public string Address { get; set; }

    public int? Priority { get; set; }

    public bool? CanPickUp { get; set; }

    public bool? CanMakeMedicalDecisions { get; set; }

    public bool? IsActive { get; set; }
}
