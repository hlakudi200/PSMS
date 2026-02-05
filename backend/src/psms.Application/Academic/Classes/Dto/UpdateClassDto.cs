using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Classes.Dto;

/// <summary>
/// Input DTO for updating a class. All fields nullable (partial update).
/// </summary>
public class UpdateClassDto
{
    [StringLength(100, MinimumLength = 2)]
    public string ClassName { get; set; }

    [Range(1, 500)]
    public int? MaxCapacity { get; set; }

    /// <summary>Set class teacher. Use AssignClassTeacher/RemoveClassTeacher for explicit control.</summary>
    public Guid? ClassTeacherId { get; set; }

    public bool? IsActive { get; set; }
}
