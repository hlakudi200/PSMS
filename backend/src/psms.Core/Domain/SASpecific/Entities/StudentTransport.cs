using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.SASpecific.Entities
{
    /// <summary>
    /// Represents a student's enrollment in school transport
    /// </summary>
    [Table("StudentTransports")]
    public class StudentTransport : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxPickupAddressLength = 500;
        public const int MaxDropoffAddressLength = 500;
        public const int MaxNotesLength = 1000;
        public const int MaxEmergencyContactLength = 100;
        public const int MaxEmergencyPhoneLength = 20;

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
        /// Reference to the transport route
        /// </summary>
        [Required]
        public Guid SchoolTransportId { get; set; }

        /// <summary>
        /// Reference to the academic year
        /// </summary>
        [Required]
        public Guid AcademicYearId { get; set; }

        /// <summary>
        /// Transport direction
        /// </summary>
        [Required]
        public TransportDirection Direction { get; set; }

        /// <summary>
        /// Pickup address
        /// </summary>
        [StringLength(MaxPickupAddressLength)]
        public string PickupAddress { get; set; }

        /// <summary>
        /// Drop-off address
        /// </summary>
        [StringLength(MaxDropoffAddressLength)]
        public string DropoffAddress { get; set; }

        /// <summary>
        /// Approximate pickup time
        /// </summary>
        public TimeSpan? PickupTime { get; set; }

        /// <summary>
        /// Status of enrollment
        /// </summary>
        [Required]
        public EnrollmentStatus Status { get; set; }

        /// <summary>
        /// Start date of enrollment
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of enrollment (if terminated)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Emergency contact name for transport
        /// </summary>
        [StringLength(MaxEmergencyContactLength)]
        public string EmergencyContactName { get; set; }

        /// <summary>
        /// Emergency contact phone for transport
        /// </summary>
        [StringLength(MaxEmergencyPhoneLength)]
        public string EmergencyContactPhone { get; set; }

        /// <summary>
        /// Additional notes
        /// </summary>
        [StringLength(MaxNotesLength)]
        public string Notes { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; }

        [ForeignKey(nameof(SchoolTransportId))]
        public virtual SchoolTransport SchoolTransport { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected StudentTransport()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public StudentTransport(
            Guid id,
            int? tenantId,
            Guid studentId,
            Guid schoolTransportId,
            Guid academicYearId,
            TransportDirection direction,
            DateTime startDate) : this()
        {
            Id = id;
            TenantId = tenantId;
            StudentId = studentId;
            SchoolTransportId = schoolTransportId;
            AcademicYearId = academicYearId;
            Direction = direction;
            StartDate = startDate;
            Status = EnrollmentStatus.Active;
        }

        /// <summary>
        /// Suspends the transport enrollment
        /// </summary>
        public void Suspend()
        {
            Status = EnrollmentStatus.Suspended;
        }

        /// <summary>
        /// Reactivates the transport enrollment
        /// </summary>
        public void Reactivate()
        {
            Status = EnrollmentStatus.Active;
        }

        /// <summary>
        /// Terminates the transport enrollment
        /// </summary>
        public void Terminate()
        {
            Status = EnrollmentStatus.Terminated;
            EndDate = DateTime.UtcNow;
        }
    }
}
