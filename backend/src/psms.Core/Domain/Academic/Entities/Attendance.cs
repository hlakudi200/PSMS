using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Represents student attendance record
    /// </summary>
    [Table("Attendances")]
    public class Attendance : CreationAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxNotesLength = 500;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Student ID
        /// </summary>
        [Required]
        public Guid StudentId { get; set; }

        /// <summary>
        /// Subject ID (optional - for subject-specific attendance)
        /// </summary>
        public Guid? SubjectId { get; set; }

        /// <summary>
        /// Class ID
        /// </summary>
        [Required]
        public Guid ClassId { get; set; }

        /// <summary>
        /// Teacher who recorded attendance (Guid type)
        /// </summary>
        [Required]
        public Guid TeacherId { get; set; }

        /// <summary>
        /// Attendance date
        /// </summary>
        [Required]
        public DateTime AttendanceDate { get; set; }

        /// <summary>
        /// Attendance status
        /// </summary>
        [Required]
        public AttendanceStatus Status { get; set; }

        /// <summary>
        /// Notes or reason
        /// </summary>
        [StringLength(MaxNotesLength)]
        public string Notes { get; set; }

        /// <summary>
        /// Online lesson ID (if attendance is for an online lesson)
        /// </summary>
        public Guid? OnlineLessonId { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; }

        [ForeignKey(nameof(SubjectId))]
        public virtual Subject Subject { get; set; }

        [ForeignKey(nameof(ClassId))]
        public virtual Class Class { get; set; }

        [ForeignKey(nameof(TeacherId))]
        public virtual Teacher Teacher { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Attendance()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Attendance(
            Guid id,
            int? tenantId,
            Guid studentId,
            Guid classId,
            Guid teacherId,
            DateTime attendanceDate,
            AttendanceStatus status) : this()
        {
            Id = id;
            TenantId = tenantId;
            StudentId = studentId;
            ClassId = classId;
            TeacherId = teacherId;
            AttendanceDate = attendanceDate;
            Status = status;
        }
    }
}
