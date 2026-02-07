using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.ReportSubjects.Dto;

/// <summary>
/// Input DTO for bulk recording marks on report subject entries.
/// </summary>
public class BulkRecordReportSubjectMarksDto
{
    [Required]
    public Guid ReportId { get; set; }

    [Required]
    public List<RecordReportSubjectMarksDto> SubjectMarks { get; set; }
}
