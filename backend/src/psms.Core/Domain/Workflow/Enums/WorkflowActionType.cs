namespace psms.Domain.Workflow.Enums;

public enum WorkflowActionType
{
    Submit = 1,
    Review = 2,
    Approve = 3,
    Reject = 4,
    Revise = 5,
    Cancel = 6,
    Recall = 7,

    /// <summary>
    /// WF-30: skip an OPTIONAL step with a mandatory reason. Behaves as a forward
    /// action (advances to the next step) but bypasses the step's guard.
    /// </summary>
    Waive = 8
}
