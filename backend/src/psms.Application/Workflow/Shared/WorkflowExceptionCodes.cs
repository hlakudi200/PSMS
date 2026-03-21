namespace psms.Workflow.Shared;

public static class WorkflowExceptionCodes
{
    // Definitions
    public const string DefinitionNotFound = "WF_DEFINITION_NOT_FOUND";
    public const string DefinitionNameDuplicate = "WF_DEFINITION_NAME_DUPLICATE";
    public const string DefinitionHasActiveInstances = "WF_DEFINITION_HAS_ACTIVE_INSTANCES";
    public const string ActiveDefinitionExists = "WF_ACTIVE_DEFINITION_EXISTS";

    // Steps
    public const string StepNotFound = "WF_STEP_NOT_FOUND";
    public const string StepOrderDuplicate = "WF_STEP_ORDER_DUPLICATE";
    public const string StepInUseByInstances = "WF_STEP_IN_USE_BY_INSTANCES";
    public const string InvalidNextStep = "WF_INVALID_NEXT_STEP";

    // Steps
    public const string DefinitionHasActiveInstancesCannotModifySteps = "WF_DEFINITION_HAS_ACTIVE_INSTANCES_CANNOT_MODIFY_STEPS";
    public const string CommentRequired = "WF_COMMENT_REQUIRED";

    // Instances
    public const string InstanceNotFound = "WF_INSTANCE_NOT_FOUND";
    public const string InstanceAlreadyExists = "WF_INSTANCE_ALREADY_EXISTS";
    public const string InstanceNotInProgress = "WF_INSTANCE_NOT_IN_PROGRESS";
    public const string InvalidTransition = "WF_INVALID_TRANSITION";
    public const string UnauthorizedAction = "WF_UNAUTHORIZED_ACTION";
    public const string NoActiveDefinition = "WF_NO_ACTIVE_DEFINITION";
    public const string RecallNotAllowed = "WF_RECALL_NOT_ALLOWED";
    public const string UserNotAssignedToStep = "WF_USER_NOT_ASSIGNED_TO_STEP";

    // Delegations
    public const string DelegationNotFound = "WF_DELEGATION_NOT_FOUND";
    public const string DelegationOverlap = "WF_DELEGATION_OVERLAP";
    public const string DelegationInvalidDates = "WF_DELEGATION_INVALID_DATES";
    public const string DelegationCannotRevokeOthers = "WF_DELEGATION_CANNOT_REVOKE_OTHERS";
    public const string DelegationSelfDelegate = "WF_DELEGATION_SELF_DELEGATE";
}
