using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace psms.Domain.Learning.Entities
{
    /// <summary>
    /// Links an existing learning material to an online lesson as pre-lesson
    /// reading (US-TCH-004). A material can prepare students for several
    /// lessons, so this is a join row rather than a column on either side.
    /// </summary>
    [Table("OnlineLessonMaterials")]
    public class OnlineLessonMaterial : CreationAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid OnlineLessonId { get; set; }

        [Required]
        public Guid LearningMaterialId { get; set; }

        [ForeignKey(nameof(OnlineLessonId))]
        public virtual OnlineLesson OnlineLesson { get; set; }

        [ForeignKey(nameof(LearningMaterialId))]
        public virtual LearningMaterial LearningMaterial { get; set; }

        protected OnlineLessonMaterial() { }

        public OnlineLessonMaterial(Guid id, int? tenantId, Guid onlineLessonId, Guid learningMaterialId)
        {
            Id = id;
            TenantId = tenantId;
            OnlineLessonId = onlineLessonId;
            LearningMaterialId = learningMaterialId;
        }
    }
}
