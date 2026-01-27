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
    /// Represents a teacher in the school
    /// </summary>
    [Table("Teachers")]
    public class Teacher : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxNameLength = 100;
        public const int MaxEmployeeNumberLength = 50;
        public const int MaxPhoneLength = 20;
        public const int MaxEmailLength = 256;
        public const int MaxQualificationsLength = 500;
        public const int MaxPhotoUrlLength = 500;
        public const int MaxEmploymentStatusLength = 50;

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
        /// Middle name (optional)
        /// </summary>
        [StringLength(MaxNameLength)]
        public string MiddleName { get; set; }

        /// <summary>
        /// Employee number (unique per tenant)
        /// </summary>
        [Required]
        [StringLength(MaxEmployeeNumberLength)]
        public string EmployeeNumber { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        [Required]
        [StringLength(MaxEmailLength)]
        public string Email { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        [StringLength(MaxPhoneLength)]
        public string Phone { get; set; }

        /// <summary>
        /// Date of joining
        /// </summary>
        public DateTime DateOfJoining { get; set; }

        /// <summary>
        /// Physical address
        /// </summary>
        public Address Address { get; set; }

        /// <summary>
        /// Qualifications and certifications
        /// </summary>
        [StringLength(MaxQualificationsLength)]
        public string Qualifications { get; set; }

        /// <summary>
        /// Comma-separated list of qualified subjects
        /// </summary>
        [StringLength(MaxQualificationsLength)]
        public string QualifiedSubjects { get; set; }

        /// <summary>
        /// Employment status (Permanent, Contract, Part-time)
        /// </summary>
        [StringLength(MaxEmploymentStatusLength)]
        public string EmploymentStatus { get; set; }

        /// <summary>
        /// Profile photo URL
        /// </summary>
        [StringLength(MaxPhotoUrlLength)]
        public string ProfilePhotoUrl { get; set; }

        /// <summary>
        /// Indicates if teacher is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual ICollection<TeacherSubject> SubjectAssignments { get; set; }
        public virtual ICollection<TeacherClass> ClassAssignments { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Teacher()
        {
            SubjectAssignments = new HashSet<TeacherSubject>();
            ClassAssignments = new HashSet<TeacherClass>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Teacher(
            Guid id,
            int? tenantId,
            long userId,
            string firstName,
            string lastName,
            string employeeNumber,
            string email) : this()
        {
            Id = id;
            TenantId = tenantId;
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            EmployeeNumber = employeeNumber;
            Email = email;
            IsActive = true;
            IsDeleted = false;
            DateOfJoining = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the full name
        /// </summary>
        public string GetFullName()
        {
            return string.IsNullOrWhiteSpace(MiddleName)
                ? $"{FirstName} {LastName}"
                : $"{FirstName} {MiddleName} {LastName}";
        }
    }
}
