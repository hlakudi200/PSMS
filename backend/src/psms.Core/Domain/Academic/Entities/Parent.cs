using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.ValueObjects;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Represents a parent or guardian of a student
    /// </summary>
    [Table("Parents")]
    public class Parent : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxNameLength = 100;
        public const int MaxPhoneLength = 20;
        public const int MaxEmailLength = 256;
        public const int MaxIdNumberLength = 13;
        public const int MaxOccupationLength = 100;
        public const int MaxEmployerLength = 200;
        public const int MaxPhotoUrlLength = 500;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// User ID from ABP Identity (long type)
        /// </summary>
        [Required]
        public long UserId { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        [Required]
        [StringLength(MaxNameLength)]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        [Required]
        [StringLength(MaxNameLength)]
        public string LastName { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        [Required]
        [StringLength(MaxEmailLength)]
        public string Email { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        [Required]
        [StringLength(MaxPhoneLength)]
        public string Phone { get; set; }

        /// <summary>
        /// ID number (South African ID)
        /// </summary>
        [StringLength(MaxIdNumberLength)]
        public string IdNumber { get; set; }

        /// <summary>
        /// Physical address
        /// </summary>
        public Address Address { get; set; }

        /// <summary>
        /// Occupation
        /// </summary>
        [StringLength(MaxOccupationLength)]
        public string Occupation { get; set; }

        /// <summary>
        /// Employer name
        /// </summary>
        [StringLength(MaxEmployerLength)]
        public string Employer { get; set; }

        /// <summary>
        /// Work phone number
        /// </summary>
        [StringLength(MaxPhoneLength)]
        public string WorkPhone { get; set; }

        /// <summary>
        /// Profile photo URL
        /// </summary>
        [StringLength(MaxPhotoUrlLength)]
        public string ProfilePhotoUrl { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual ICollection<StudentParent> StudentLinks { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Parent()
        {
            StudentLinks = new HashSet<StudentParent>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Parent(
            Guid id,
            int? tenantId,
            long userId,
            string firstName,
            string lastName,
            string email,
            string phone) : this()
        {
            Id = id;
            TenantId = tenantId;
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Phone = phone;
            IsDeleted = false;
        }

        /// <summary>
        /// Gets the full name
        /// </summary>
        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
