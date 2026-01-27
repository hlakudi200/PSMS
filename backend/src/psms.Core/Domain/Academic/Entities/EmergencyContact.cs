using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Represents an emergency contact for a student
    /// </summary>
    [Table("EmergencyContacts")]
    public class EmergencyContact : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxNameLength = 100;
        public const int MaxPhoneLength = 20;
        public const int MaxEmailLength = 256;
        public const int MaxAddressLength = 500;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Reference to the student
        /// </summary>
        [Required]
        public Guid StudentId { get; set; }

        /// <summary>
        /// First name of the contact
        /// </summary>
        [Required]
        [StringLength(MaxNameLength)]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of the contact
        /// </summary>
        [Required]
        [StringLength(MaxNameLength)]
        public string LastName { get; set; }

        /// <summary>
        /// Relationship to the student
        /// </summary>
        [Required]
        public RelationshipType Relationship { get; set; }

        /// <summary>
        /// Primary phone number
        /// </summary>
        [Required]
        [StringLength(MaxPhoneLength)]
        public string PrimaryPhone { get; set; }

        /// <summary>
        /// Secondary/alternative phone number
        /// </summary>
        [StringLength(MaxPhoneLength)]
        public string SecondaryPhone { get; set; }

        /// <summary>
        /// Work phone number
        /// </summary>
        [StringLength(MaxPhoneLength)]
        public string WorkPhone { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        [StringLength(MaxEmailLength)]
        public string Email { get; set; }

        /// <summary>
        /// Physical address
        /// </summary>
        [StringLength(MaxAddressLength)]
        public string Address { get; set; }

        /// <summary>
        /// Priority order for contacting (1 = first contact, 2 = second, etc.)
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Whether this contact is authorized to pick up the student
        /// </summary>
        public bool CanPickUp { get; set; }

        /// <summary>
        /// Whether this contact is authorized to make medical decisions
        /// </summary>
        public bool CanMakeMedicalDecisions { get; set; }

        /// <summary>
        /// Whether this contact is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected EmergencyContact()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public EmergencyContact(
            Guid id,
            int? tenantId,
            Guid studentId,
            string firstName,
            string lastName,
            RelationshipType relationship,
            string primaryPhone,
            int priority = 1) : this()
        {
            Id = id;
            TenantId = tenantId;
            StudentId = studentId;
            FirstName = firstName;
            LastName = lastName;
            Relationship = relationship;
            PrimaryPhone = primaryPhone;
            Priority = priority;
            IsActive = true;
        }

        /// <summary>
        /// Gets the full name of the contact
        /// </summary>
        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
