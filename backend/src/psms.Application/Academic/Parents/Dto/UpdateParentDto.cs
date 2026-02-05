using psms.Academic.Shared;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Parents.Dto;

/// <summary>
/// Input DTO for updating a parent. All fields nullable (partial update).
/// </summary>
public class UpdateParentDto
{
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; }

    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; }

    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; }

    [StringLength(20)]
    public string Phone { get; set; }

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
