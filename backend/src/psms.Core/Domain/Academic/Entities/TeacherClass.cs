using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Junction table linking teachers to classes
    /// </summary>
    [Table("TeacherClasses")]
    public class TeacherClass : Entity<Guid>
    {
        /// <summary>
        /// Teacher ID
        /// </summary>
        [Required]
        public Guid TeacherId { get; set; }

        /// <summary>
        /// Class ID
        /// </summary>
        [Required]
        public Guid ClassId { get; set; }

        /// <summary>
        /// Subject ID (teacher teaches this subject to this class)
        /// </summary>
        [Required]
        public Guid SubjectId { get; set; }

        /// <summary>
        /// Is this the class teacher (form teacher/register teacher)
        /// </summary>
        public bool IsClassTeacher { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(TeacherId))]
        public virtual Teacher Teacher { get; set; }

        [ForeignKey(nameof(ClassId))]
        public virtual Class Class { get; set; }

        [ForeignKey(nameof(SubjectId))]
        public virtual Subject Subject { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected TeacherClass()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public TeacherClass(
            Guid id,
            Guid teacherId,
            Guid classId,
            Guid subjectId) : this()
        {
            Id = id;
            TeacherId = teacherId;
            ClassId = classId;
            SubjectId = subjectId;
            IsClassTeacher = false;
        }
    }
}
