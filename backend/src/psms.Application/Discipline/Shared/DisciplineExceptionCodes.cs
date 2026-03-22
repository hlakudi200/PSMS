namespace psms.Discipline.Shared;

/// <summary>
/// Exception codes for the Discipline module.
/// </summary>
public static class DisciplineExceptionCodes
{
    public const string CaseNotFound = "DC_CASE_NOT_FOUND";
    public const string InvalidStatusTransition = "DC_INVALID_STATUS";
    public const string CaseNumberDuplicate = "DC_CASE_NUMBER_DUPLICATE";
    public const string OutcomeRequired = "DC_OUTCOME_REQUIRED";
}
