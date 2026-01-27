using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.SASpecific.Entities
{
    /// <summary>
    /// Represents a school transport route/vehicle
    /// </summary>
    [Table("SchoolTransports")]
    public class SchoolTransport : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxRouteNameLength = 200;
        public const int MaxDescriptionLength = 1000;
        public const int MaxVehicleNumberLength = 20;
        public const int MaxDriverNameLength = 100;
        public const int MaxDriverPhoneLength = 20;
        public const int MaxAreasCoveredLength = 2000;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Name of the transport route
        /// </summary>
        [Required]
        [StringLength(MaxRouteNameLength)]
        public string RouteName { get; set; }

        /// <summary>
        /// Description of the route
        /// </summary>
        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        /// <summary>
        /// Vehicle registration number
        /// </summary>
        [StringLength(MaxVehicleNumberLength)]
        public string VehicleNumber { get; set; }

        /// <summary>
        /// Type of transport
        /// </summary>
        [Required]
        public TransportType TransportType { get; set; }

        /// <summary>
        /// Maximum capacity of the vehicle
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Current number of enrolled students
        /// </summary>
        public int CurrentEnrollment { get; set; }

        /// <summary>
        /// Name of the driver
        /// </summary>
        [StringLength(MaxDriverNameLength)]
        public string DriverName { get; set; }

        /// <summary>
        /// Driver's phone number
        /// </summary>
        [StringLength(MaxDriverPhoneLength)]
        public string DriverPhone { get; set; }

        /// <summary>
        /// Areas/suburbs covered by this route (JSON array or comma-separated)
        /// </summary>
        [StringLength(MaxAreasCoveredLength)]
        public string AreasCovered { get; set; }

        /// <summary>
        /// Morning pickup start time
        /// </summary>
        public TimeSpan? MorningPickupTime { get; set; }

        /// <summary>
        /// Afternoon departure time from school
        /// </summary>
        public TimeSpan? AfternoonDepartureTime { get; set; }

        /// <summary>
        /// Monthly fee for this route (ZAR)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyFee { get; set; }

        /// <summary>
        /// Whether the route is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Collections
        public virtual ICollection<StudentTransport> StudentEnrollments { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected SchoolTransport()
        {
            StudentEnrollments = new HashSet<StudentTransport>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public SchoolTransport(
            Guid id,
            int? tenantId,
            string routeName,
            TransportType transportType,
            int capacity,
            decimal monthlyFee) : this()
        {
            Id = id;
            TenantId = tenantId;
            RouteName = routeName;
            TransportType = transportType;
            Capacity = capacity;
            MonthlyFee = monthlyFee;
            CurrentEnrollment = 0;
            IsActive = true;
            IsDeleted = false;
        }

        /// <summary>
        /// Checks if the route has capacity
        /// </summary>
        public bool HasCapacity()
        {
            return CurrentEnrollment < Capacity;
        }

        /// <summary>
        /// Enrolls a student (increments count)
        /// </summary>
        public void EnrollStudent()
        {
            if (!HasCapacity())
                throw new InvalidOperationException("Transport route is at full capacity.");

            CurrentEnrollment++;
        }

        /// <summary>
        /// Removes a student (decrements count)
        /// </summary>
        public void RemoveStudent()
        {
            if (CurrentEnrollment > 0)
                CurrentEnrollment--;
        }

        /// <summary>
        /// Deactivates the route
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Activates the route
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }
    }
}
