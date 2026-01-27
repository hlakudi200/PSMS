using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Junction table linking grades to subjects
    /// </summary>
    [Table("GradeSubjects")]
    public class GradeSubject : Entity<Guid>
    {
        /// <summary>
        /// Grade ID
        /// </summary>
        [Required]
        public Guid GradeId { get; set; }

        /// <summary>
        /// Subject ID
        /// </summary>
        [Required]
        public Guid SubjectId { get; set; }

        /// <summary>
        /// Is this subject required for this grade
        /// </summary>
        public bool IsRequired { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(GradeId))]
        public virtual Grade Grade { get; set; }

        [ForeignKey(nameof(SubjectId))]
        public virtual Subject Subject { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected GradeSubject()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public GradeSubject(
            Guid id,
            Guid gradeId,
            Guid subjectId,
            bool isRequired) : this()
        {
            Id = id;
            GradeId = gradeId;
            SubjectId = subjectId;
            IsRequired = isRequired;
        }
    }
}
