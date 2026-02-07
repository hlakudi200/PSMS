using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Shared;

/// <summary>
/// DTO for the Address value object. Shared by Teacher and Parent DTOs.
/// </summary>
public class AddressDto
{
    [StringLength(200)]
    public string StreetAddress { get; set; }

    [StringLength(100)]
    public string Suburb { get; set; }

    [StringLength(100)]
    public string City { get; set; }

    [StringLength(50)]
    public string Province { get; set; }

    [StringLength(10)]
    public string PostalCode { get; set; }

    [StringLength(50)]
    public string Country { get; set; }
}
