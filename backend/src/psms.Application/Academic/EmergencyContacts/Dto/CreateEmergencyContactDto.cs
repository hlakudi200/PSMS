using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.EmergencyContacts.Dto;

public class CreateEmergencyContactDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; }

    [Required]
    public RelationshipType Relationship { get; set; }

    [Required]
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

    public int Priority { get; set; } = 1;

    public bool CanPickUp { get; set; }

    public bool CanMakeMedicalDecisions { get; set; }
}
