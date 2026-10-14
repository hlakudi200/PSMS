using System;
using System.Collections.Generic;

namespace psms.Financial.Statements.Dto;

/// <summary>An academic year a statement can be drawn for, with its terms.</summary>
public class FeeStatementYearDto
{
    public Guid AcademicYearId { get; set; }
    public string YearName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public List<FeeStatementTermDto> Terms { get; set; } = new();
}

public class FeeStatementTermDto
{
    public Guid TermId { get; set; }
    public string TermName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; }
}
