using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Junction table linking students to subjects
    /// </summary>
    [Table("StudentSubjects")]
    public class StudentSubject : Entity<Guid>
    {
        /// <summary>
        /// Student ID
        /// </summary>
        [Required]
        public Guid StudentId { get; set; }

        /// <summary>
        /// Subject ID
        /// </summary>
        [Required]
        public Guid SubjectId { get; set; }

        /// <summary>
        /// Academic year
        /// </summary>
        [Required]
        public Guid AcademicYearId { get; set; }

        /// <summary>
        /// Enrollment date
        /// </summary>
        [Required]
        public DateTime EnrollmentDate { get; set; }

        /// <summary>
        /// Is enrollment active
        /// </summary>
        public bool IsActive { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; }

        [ForeignKey(nameof(SubjectId))]
        public virtual Subject Subject { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected StudentSubject()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public StudentSubject(
            Guid id,
            Guid studentId,
            Guid subjectId,
            Guid academicYearId,
            DateTime enrollmentDate) : this()
        {
            Id = id;
            StudentId = studentId;
            SubjectId = subjectId;
            AcademicYearId = academicYearId;
            EnrollmentDate = enrollmentDate;
            IsActive = true;
        }
    }
}
