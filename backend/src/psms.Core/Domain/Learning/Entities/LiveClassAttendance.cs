using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace psms.Domain.Learning.Entities
{
    /// <summary>
    /// One row per (in-app live lesson, participant) — records that a participant
    /// attended the live class and when (LC-05). Distinct rows give the true
    /// attendee roll-call, vs the old peak-concurrent count. Fed by LiveKit
    /// participant_joined / participant_left webhooks.
    /// </summary>
    public class LiveClassAttendance : CreationAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid OnlineLessonId { get; set; }

        /// <summary>Stable LiveKit participant identity, e.g. "user-42".</summary>
        [Required]
        [StringLength(128)]
        public string ParticipantIdentity { get; set; }

        /// <summary>Display name at join time (best-effort).</summary>
        [StringLength(256)]
        public string DisplayName { get; set; }

        /// <summary>First time this participant joined the session.</summary>
        public DateTime FirstJoinedAt { get; set; }

        /// <summary>Most recent time they left (null while still considered present).</summary>
        public DateTime? LastLeftAt { get; set; }

        protected LiveClassAttendance() { }

        public LiveClassAttendance(Guid id, int? tenantId, Guid onlineLessonId,
            string participantIdentity, string displayName, DateTime firstJoinedAt) : base()
        {
            Id = id;
            TenantId = tenantId;
            OnlineLessonId = onlineLessonId;
            ParticipantIdentity = participantIdentity;
            DisplayName = displayName;
            FirstJoinedAt = firstJoinedAt;
        }
    }
}
