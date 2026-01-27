using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Junction table linking teachers to subjects they teach
    /// </summary>
    [Table("TeacherSubjects")]
    public class TeacherSubject : Entity<Guid>
    {
        /// <summary>
        /// Teacher ID
        /// </summary>
        [Required]
        public Guid TeacherId { get; set; }

        /// <summary>
        /// Subject ID
        /// </summary>
        [Required]
        public Guid SubjectId { get; set; }

        /// <summary>
        /// Grade ID
        /// </summary>
        [Required]
        public Guid GradeId { get; set; }

        /// <summary>
        /// Is this the primary subject for the teacher
        /// </summary>
        public bool IsPrimary { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(TeacherId))]
        public virtual Teacher Teacher { get; set; }

        [ForeignKey(nameof(SubjectId))]
        public virtual Subject Subject { get; set; }

        [ForeignKey(nameof(GradeId))]
        public virtual Grade Grade { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected TeacherSubject()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public TeacherSubject(
            Guid id,
            Guid teacherId,
            Guid subjectId,
            Guid gradeId) : this()
        {
            Id = id;
            TeacherId = teacherId;
            SubjectId = subjectId;
            GradeId = gradeId;
            IsPrimary = false;
        }
    }
}
