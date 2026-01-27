using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Junction table linking students to their parents/guardians
    /// </summary>
    [Table("StudentParents")]
    public class StudentParent : Entity<Guid>
    {
        /// <summary>
        /// Student ID
        /// </summary>
        [Required]
        public Guid StudentId { get; set; }

        /// <summary>
        /// Parent ID
        /// </summary>
        [Required]
        public Guid ParentId { get; set; }

        /// <summary>
        /// Relationship type (Father, Mother, Guardian, etc.)
        /// </summary>
        [Required]
        public RelationshipType RelationshipType { get; set; }

        /// <summary>
        /// Is this the primary contact for the student
        /// </summary>
        public bool IsPrimaryContact { get; set; }

        /// <summary>
        /// Is this parent financially responsible
        /// </summary>
        public bool IsFinanciallyResponsible { get; set; }

        /// <summary>
        /// Can this parent pick up the student
        /// </summary>
        public bool CanPickupStudent { get; set; }

        /// <summary>
        /// Lives with student
        /// </summary>
        public bool LivesWithStudent { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; }

        [ForeignKey(nameof(ParentId))]
        public virtual Parent Parent { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected StudentParent()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public StudentParent(
            Guid id,
            Guid studentId,
            Guid parentId,
            RelationshipType relationshipType) : this()
        {
            Id = id;
            StudentId = studentId;
            ParentId = parentId;
            RelationshipType = relationshipType;
            IsPrimaryContact = false;
            IsFinanciallyResponsible = false;
            CanPickupStudent = true;
            LivesWithStudent = false;
        }
    }
}
