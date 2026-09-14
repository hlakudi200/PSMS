using System;
using System.Collections.Generic;

namespace psms.Academic.Timetables.Dto;

public class GenerateTimetablesResultDto
{
    public List<GeneratedClassTimetableDto> Classes { get; set; } = new();
    public List<UnplacedLessonDto> Unplaced { get; set; } = new();
    public int TotalClasses => Classes.Count;
    public int TotalSlotsPlaced { get; set; }
    public int TotalSlotsRequested { get; set; }

    /// <summary>
    /// Set when there was nothing to generate (e.g. no active classes for
    /// the selected academic year), so the UI can explain the empty result
    /// instead of it reading as a silent failure.
    /// </summary>
    public string Message { get; set; }
}

public class GeneratedClassTimetableDto
{
    public Guid ClassId { get; set; }
    public string ClassName { get; set; }
    /// <summary>Null when no draft was created — nothing was requested for
    /// this class, or nothing could be placed at all (see Unplaced).</summary>
    public Guid? TimetableId { get; set; }
    public int SlotsPlaced { get; set; }
    public int SlotsRequested { get; set; }
}

public class UnplacedLessonDto
{
    public Guid ClassId { get; set; }
    public string ClassName { get; set; }
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; }
    public Guid? TeacherId { get; set; }
    public string TeacherName { get; set; }
    public string Reason { get; set; }
}
