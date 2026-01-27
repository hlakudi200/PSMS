using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Admissions.Entities
{
    /// <summary>
    /// Represents a parent/guardian associated with an application
    /// </summary>
    [Table("ApplicantParents")]
    public class ApplicantParent : Entity<Guid>
    {
        public const int MaxNameLength = 100;
        public const int MaxIdNumberLength = 13;
        public const int MaxEmailLength = 256;
        public const int MaxPhoneLength = 20;
        public const int MaxOccupationLength = 100;
        public const int MaxEmployerLength = 200;
        public const int MaxAddressLength = 200;
        public const int MaxCityLength = 100;
        public const int MaxProvinceLength = 50;
        public const int MaxPostalCodeLength = 10;

        [Required]
        public Guid ApplicationId { get; set; }

        [Required]
        public RelationshipType Relationship { get; set; }

        [Required]
        [StringLength(MaxNameLength)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(MaxNameLength)]
        public string LastName { get; set; }

        [StringLength(MaxIdNumberLength)]
        public string IdNumber { get; set; }

        [Required]
        [StringLength(MaxEmailLength)]
        public string Email { get; set; }

        [Required]
        [StringLength(MaxPhoneLength)]
        public string PhoneNumber { get; set; }

        [StringLength(MaxPhoneLength)]
        public string AlternatePhone { get; set; }

        [StringLength(MaxAddressLength)]
        public string StreetAddress { get; set; }

        [StringLength(MaxCityLength)]
        public string Suburb { get; set; }

        [StringLength(MaxCityLength)]
        public string City { get; set; }

        [StringLength(MaxProvinceLength)]
        public string Province { get; set; }

        [StringLength(MaxPostalCodeLength)]
        public string PostalCode { get; set; }

        [StringLength(MaxOccupationLength)]
        public string Occupation { get; set; }

        [StringLength(MaxEmployerLength)]
        public string Employer { get; set; }

        public bool IsPrimaryContact { get; set; }

        public bool IsFinanciallyResponsible { get; set; }

        [ForeignKey(nameof(ApplicationId))]
        public virtual Application Application { get; set; }

        protected ApplicantParent() { }

        public ApplicantParent(Guid id, Guid applicationId, RelationshipType relationship,
            string firstName, string lastName, string email, string phoneNumber)
        {
            Id = id;
            ApplicationId = applicationId;
            Relationship = relationship;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
        }

        public string GetFullName() => $"{FirstName} {LastName}";
    }
}
