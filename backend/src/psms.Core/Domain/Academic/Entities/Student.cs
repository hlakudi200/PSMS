using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;
using psms.Domain.Shared.ValueObjects;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Represents a student (learner) enrolled in the school with SA-specific fields
    /// </summary>
    [Table("Students")]
    public class Student : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxNameLength = 100;
        public const int MaxAdmissionNumberLength = 50;
        public const int MaxIdNumberLength = 13;
        public const int MaxPassportNumberLength = 50;
        public const int MaxPhoneLength = 20;
        public const int MaxEmailLength = 256;
        public const int MaxPhotoUrlLength = 500;
        public const int MaxNotesLength = 2000;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// First name of the student
        /// </summary>
        [Required]
        [StringLength(MaxNameLength)]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of the student
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
        /// Date of birth
        /// </summary>
        [Required]
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Gender
        /// </summary>
        [Required]
        public Gender Gender { get; set; }

        /// <summary>
        /// South African ID number (for SA citizens)
        /// </summary>
        [StringLength(MaxIdNumberLength)]
        public string IdNumber { get; set; }

        /// <summary>
        /// Passport number (for non-SA citizens)
        /// </summary>
        [StringLength(MaxPassportNumberLength)]
        public string PassportNumber { get; set; }

        /// <summary>
        /// Indicates if student is a South African citizen
        /// </summary>
        public bool IsSACitizen { get; set; }

        /// <summary>
        /// Unique admission number for the student
        /// </summary>
        [Required]
        [StringLength(MaxAdmissionNumberLength)]
        public string AdmissionNumber { get; set; }

        /// <summary>
        /// Date when student was admitted
        /// </summary>
        [Required]
        public DateTime AdmissionDate { get; set; }

        /// <summary>
        /// Current grade the student is enrolled in
        /// </summary>
        [Required]
        public Guid CurrentGradeId { get; set; }

        /// <summary>
        /// Current class the student is enrolled in
        /// </summary>
        [Required]
        public Guid CurrentClassId { get; set; }

        /// <summary>
        /// The AbpUser login account provisioned for this student (LC-07).
        /// Nullable: students created before LC-07 (or via paths that don't
        /// provision a login) have no account. Set via <see cref="LinkUser"/>.
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// Physical address
        /// </summary>
        public Address PhysicalAddress { get; set; }

        /// <summary>
        /// Postal address (if different from physical)
        /// </summary>
        public Address PostalAddress { get; set; }

        /// <summary>
        /// Contact phone number
        /// </summary>
        [StringLength(MaxPhoneLength)]
        public string Phone { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        [StringLength(MaxEmailLength)]
        public string Email { get; set; }

        /// <summary>
        /// Profile photo URL
        /// </summary>
        [StringLength(MaxPhotoUrlLength)]
        public string ProfilePhotoUrl { get; set; }

        /// <summary>
        /// Emergency contact name
        /// </summary>
        [StringLength(MaxNameLength)]
        public string EmergencyContactName { get; set; }

        /// <summary>
        /// Emergency contact phone
        /// </summary>
        [StringLength(MaxPhoneLength)]
        public string EmergencyContactPhone { get; set; }

        /// <summary>
        /// Medical conditions or allergies
        /// </summary>
        [StringLength(MaxNotesLength)]
        public string MedicalConditions { get; set; }

        /// <summary>
        /// POPIA consent given (SA data protection)
        /// </summary>
        public bool POPIAConsentGiven { get; set; }

        /// <summary>
        /// Date when POPIA consent was given
        /// </summary>
        public DateTime? POPIAConsentDate { get; set; }

        /// <summary>
        /// Indicates if student is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(CurrentGradeId))]
        public virtual Grade CurrentGrade { get; set; }

        [ForeignKey(nameof(CurrentClassId))]
        public virtual Class CurrentClass { get; set; }

        public virtual ICollection<StudentParent> ParentLinks { get; set; }
        public virtual ICollection<StudentSubject> SubjectEnrollments { get; set; }
        public virtual ICollection<Attendance> Attendances { get; set; }
        public virtual ICollection<POPIAConsent> POPIAConsents { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Student()
        {
            ParentLinks = new HashSet<StudentParent>();
            SubjectEnrollments = new HashSet<StudentSubject>();
            Attendances = new HashSet<Attendance>();
            POPIAConsents = new HashSet<POPIAConsent>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Student(
            Guid id,
            int? tenantId,
            string firstName,
            string lastName,
            DateTime dateOfBirth,
            Gender gender,
            string admissionNumber,
            DateTime admissionDate,
            Guid currentGradeId,
            Guid currentClassId) : this()
        {
            Id = id;
            TenantId = tenantId;
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            AdmissionNumber = admissionNumber;
            AdmissionDate = admissionDate;
            CurrentGradeId = currentGradeId;
            CurrentClassId = currentClassId;
            IsActive = true;
            IsDeleted = false;
        }

        /// <summary>
        /// Gets the full name of the student
        /// </summary>
        public string GetFullName()
        {
            return string.IsNullOrWhiteSpace(MiddleName)
                ? $"{FirstName} {LastName}"
                : $"{FirstName} {MiddleName} {LastName}";
        }

        /// <summary>
        /// Calculates the age of the student
        /// </summary>
        public int GetAge()
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age))
                age--;
            return age;
        }

        /// <summary>Links this student to their AbpUser login account (LC-07).</summary>
        public void LinkUser(long userId)
        {
            UserId = userId;
        }
    }
}
