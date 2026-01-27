using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Represents a time slot in a timetable
    /// </summary>
    [Table("TimetableSlots")]
    public class TimetableSlot : Entity<Guid>
    {
        public const int MaxRoomNumberLength = 50;

        /// <summary>
        /// Timetable this slot belongs to
        /// </summary>
        [Required]
        public Guid TimetableId { get; set; }

        /// <summary>
        /// Day of week
        /// </summary>
        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>
        /// Period number (1, 2, 3, etc.)
        /// </summary>
        [Required]
        public int PeriodNumber { get; set; }

        /// <summary>
        /// Start time of period
        /// </summary>
        [Required]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// End time of period
        /// </summary>
        [Required]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Subject for this slot
        /// </summary>
        [Required]
        public Guid SubjectId { get; set; }

        /// <summary>
        /// Teacher for this slot
        /// </summary>
        [Required]
        public Guid TeacherId { get; set; }

        /// <summary>
        /// Room number or location
        /// </summary>
        [StringLength(MaxRoomNumberLength)]
        public string RoomNumber { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(TimetableId))]
        public virtual Timetable Timetable { get; set; }

        [ForeignKey(nameof(SubjectId))]
        public virtual Subject Subject { get; set; }

        [ForeignKey(nameof(TeacherId))]
        public virtual Teacher Teacher { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected TimetableSlot()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public TimetableSlot(
            Guid id,
            Guid timetableId,
            DayOfWeek dayOfWeek,
            int periodNumber,
            TimeSpan startTime,
            TimeSpan endTime,
            Guid subjectId,
            Guid teacherId) : this()
        {
            Id = id;
            TimetableId = timetableId;
            DayOfWeek = dayOfWeek;
            PeriodNumber = periodNumber;
            StartTime = startTime;
            EndTime = endTime;
            SubjectId = subjectId;
            TeacherId = teacherId;
        }
    }
}
