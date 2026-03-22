namespace psms.HR.Shared;

/// <summary>
/// Exception codes for the HR module.
/// </summary>
public static class HRExceptionCodes
{
    // StaffLeaveRequest
    public const string LeaveNotFound = "LR_NOT_FOUND";
    public const string InvalidStatus = "LR_INVALID_STATUS";
    public const string OverlappingLeave = "LR_OVERLAP";
    public const string InvalidDateRange = "LR_INVALID_DATES";
    public const string LeaveNumberDuplicate = "LR_NUMBER_DUPLICATE";
}
