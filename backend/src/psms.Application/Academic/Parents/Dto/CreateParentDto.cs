using psms.Academic.Shared;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Parents.Dto;

/// <summary>
/// Input DTO for creating a new parent.
/// </summary>
public class CreateParentDto
{
    [Required]
    public long UserId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; }

    [Required]
    [StringLength(20)]
    public string Phone { get; set; }

    /// <summary>South African ID number (13 digits)</summary>
    [StringLength(13)]
    public string IdNumber { get; set; }

    public AddressDto Address { get; set; }

    [StringLength(100)]
    public string Occupation { get; set; }

    [StringLength(200)]
    public string Employer { get; set; }

    [StringLength(20)]
    public string WorkPhone { get; set; }
}
