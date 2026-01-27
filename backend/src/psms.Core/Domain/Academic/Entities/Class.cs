using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Represents a class/section within a grade
    /// </summary>
    [Table("Classes")]
    public class Class : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxClassNameLength = 100;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Class name (e.g., "1A", "Grade 1A")
        /// </summary>
        [Required]
        [StringLength(MaxClassNameLength)]
        public string ClassName { get; set; }

        /// <summary>
        /// Grade this class belongs to
        /// </summary>
        [Required]
        public Guid GradeId { get; set; }

        /// <summary>
        /// Academic year
        /// </summary>
        [Required]
        public Guid AcademicYearId { get; set; }

        /// <summary>
        /// Maximum capacity of students
        /// </summary>
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Class teacher (form teacher) ID
        /// </summary>
        public Guid? ClassTeacherId { get; set; }

        /// <summary>
        /// Indicates if class is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(GradeId))]
        public virtual Grade Grade { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; }

        [ForeignKey(nameof(ClassTeacherId))]
        public virtual Teacher ClassTeacher { get; set; }

        public virtual ICollection<Student> Students { get; set; }
        public virtual ICollection<TeacherClass> TeacherAssignments { get; set; }
        public virtual ICollection<Timetable> Timetables { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Class()
        {
            Students = new HashSet<Student>();
            TeacherAssignments = new HashSet<TeacherClass>();
            Timetables = new HashSet<Timetable>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Class(
            Guid id,
            int? tenantId,
            string className,
            Guid gradeId,
            Guid academicYearId,
            int maxCapacity) : this()
        {
            Id = id;
            TenantId = tenantId;
            ClassName = className;
            GradeId = gradeId;
            AcademicYearId = academicYearId;
            MaxCapacity = maxCapacity;
            IsActive = true;
            IsDeleted = false;
        }
    }
}
