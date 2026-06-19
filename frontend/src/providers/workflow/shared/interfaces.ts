// Pagination (re-export from academic shared for convenience)
export interface IPagedAndSortedResultRequest {
  maxResultCount?: number;
  skipCount?: number;
  sorting?: string;
  keyword?: string;
  [key: string]: unknown;
}

export interface IPagedResult<T> {
  totalCount: number;
  items: T[];
}

export interface IListResult<T> {
  items: T[];
}

// ============================================================
// Enums
// ============================================================
export enum WorkflowStatus {
  NotStarted = 1,
  InProgress = 2,
  Completed = 3,
  Rejected = 4,
  Cancelled = 5,
  Recalled = 6,
}

export enum WorkflowActionType {
  Submit = 1,
  Review = 2,
  Approve = 3,
  Reject = 4,
  Revise = 5,
  Cancel = 6,
  Recall = 7,
}

export enum WorkflowEntityType {
  Application = 1,
  Report = 2,
  FeeWaiver = 3,
  Attendance = 4,
  LearningMaterial = 5,
  StudentTransfer = 6,
  Disciplinary = 7,
  StaffLeave = 8,
  FieldTrip = 9,
  ExpenseRequest = 10,
}

export const WorkflowStatusLabels: Record<number, string> = {
  [WorkflowStatus.NotStarted]: 'Not Started',
  [WorkflowStatus.InProgress]: 'In Progress',
  [WorkflowStatus.Completed]: 'Completed',
  [WorkflowStatus.Rejected]: 'Rejected',
  [WorkflowStatus.Cancelled]: 'Cancelled',
  [WorkflowStatus.Recalled]: 'Recalled',
};

export const WorkflowActionTypeLabels: Record<number, string> = {
  [WorkflowActionType.Submit]: 'Submit',
  [WorkflowActionType.Review]: 'Review',
  [WorkflowActionType.Approve]: 'Approve',
  [WorkflowActionType.Reject]: 'Reject',
  [WorkflowActionType.Revise]: 'Revise',
  [WorkflowActionType.Cancel]: 'Cancel',
  [WorkflowActionType.Recall]: 'Recall',
};

export const WorkflowEntityTypeLabels: Record<number, string> = {
  [WorkflowEntityType.Application]: 'Application',
  [WorkflowEntityType.Report]: 'Report',
  [WorkflowEntityType.FeeWaiver]: 'Fee Waiver',
  [WorkflowEntityType.Attendance]: 'Attendance',
  [WorkflowEntityType.LearningMaterial]: 'Learning Material',
  [WorkflowEntityType.StudentTransfer]: 'Student Transfer',
  [WorkflowEntityType.Disciplinary]: 'Disciplinary',
  [WorkflowEntityType.StaffLeave]: 'Staff Leave',
  [WorkflowEntityType.FieldTrip]: 'Field Trip',
  [WorkflowEntityType.ExpenseRequest]: 'Expense Request',
};

// ============================================================
// WorkflowDefinition
// ============================================================
export interface IWorkflowDefinition {
  id: string;
  name: string;
  description?: string;
  entityType: number;
  isActive: boolean;
  version: number;
  steps: IWorkflowStep[];
  creationTime: string;
  lastModificationTime?: string;
}

export interface IWorkflowDefinitionList {
  id: string;
  name: string;
  entityType: number;
  isActive: boolean;
  version: number;
  stepCount: number;
  creationTime: string;
}

export interface ICreateWorkflowDefinition {
  name: string;
  description?: string;
  entityType: number;
  isActive: boolean;
}

export interface IUpdateWorkflowDefinition {
  name?: string;
  description?: string;
}

export interface IGetWorkflowDefinitionsInput extends IPagedAndSortedResultRequest {
  entityType?: number;
  isActive?: boolean;
  search?: string;
}

// ============================================================
// WorkflowStep
// ============================================================
export interface IWorkflowStep {
  id: string;
  workflowDefinitionId: string;
  stepOrder: number;
  name: string;
  description?: string;
  assignedRole: string;
  actionType: number;
  isTerminal: boolean;
  nextStepOnApprove?: number;
  nextStepOnReject?: number;
  isCommentRequired: boolean;
  assignedUserId?: number;
  slaHours?: number;
  guardExpression?: string;
}

export interface ICreateWorkflowStep {
  workflowDefinitionId: string;
  stepOrder: number;
  name: string;
  description?: string;
  assignedRole: string;
  actionType: number;
  isTerminal: boolean;
  nextStepOnApprove?: number;
  nextStepOnReject?: number;
  isCommentRequired: boolean;
  assignedUserId?: number;
  slaHours?: number;
  guardExpression?: string;
}

export interface IUpdateWorkflowStep {
  name?: string;
  description?: string;
  assignedRole?: string;
  actionType?: number;
  isTerminal?: boolean;
  nextStepOnApprove?: number;
  clearNextStepOnApprove?: boolean;
  nextStepOnReject?: number;
  clearNextStepOnReject?: boolean;
  isCommentRequired?: boolean;
  assignedUserId?: number;
  clearAssignedUserId?: boolean;
  slaHours?: number;
  clearSlaHours?: boolean;
  guardExpression?: string;
  clearGuardExpression?: boolean;
}

export interface IReorderWorkflowSteps {
  workflowDefinitionId: string;
  stepIds: string[];
}

// ============================================================
// WorkflowInstance
// ============================================================
export interface IWorkflowInstance {
  id: string;
  workflowDefinitionId: string;
  workflowDefinitionName: string;
  entityType: number;
  entityId: string;
  status: number;
  currentStepOrder: number;
  currentStepId?: string;
  currentStepName?: string;
  currentStepAssignedRole?: string;
  /** WF-22: current step's action type, so the action modal offers only valid choices. */
  currentStepActionType?: number;
  /** WF-22: whether the current step requires a comment (for the detail page action modal). */
  currentStepIsCommentRequired?: boolean;
  workflowDefinitionVersion: number;
  currentStepDueDate?: string;
  isOverdue: boolean;
  startedDate?: string;
  completedDate?: string;
  completedByUserId?: number;
  completionComment?: string;
  transitions: IWorkflowTransition[];
  creationTime: string;
  lastModificationTime?: string;
}

export interface IWorkflowEntitySummaryField {
  label: string;
  value: string;
}

export interface IWorkflowEntitySummarySection {
  heading: string;
  fields: IWorkflowEntitySummaryField[];
}

export interface IWorkflowEntitySummaryTable {
  heading: string;
  columns: string[];
  rows: string[][];
}

/** WF-09/WF-18: the full record an instance is about — grouped sections + child tables. */
export interface IWorkflowEntitySummary {
  title: string;
  subtitle?: string;
  sections: IWorkflowEntitySummarySection[];
  tables: IWorkflowEntitySummaryTable[];
}

export interface IWorkflowInstanceList {
  id: string;
  workflowDefinitionId: string;
  workflowDefinitionName: string;
  entityType: number;
  entityId: string;
  /** WF-20: concise label for the linked record (e.g. student name) for at-a-glance rows. */
  subjectLabel?: string;
  status: number;
  currentStepOrder: number;
  currentStepName?: string;
  currentStepAssignedRole?: string;
  /** WF-22: current step's action type, so the action modal offers only valid choices. */
  currentStepActionType?: number;
  currentStepIsCommentRequired?: boolean;
  currentStepDueDate?: string;
  isOverdue: boolean;
  startedDate?: string;
  completedDate?: string;
  creationTime: string;
}

export interface IStartWorkflow {
  entityType: number;
  entityId: string;
  workflowDefinitionId?: string;
}

export interface IAdvanceWorkflow {
  action: number;
  comment?: string;
  attachmentUrl?: string;
}

export interface IBatchAdvance {
  instanceIds: string[];
  action: number;
  comment?: string;
}

export interface IBatchAdvanceResult {
  successCount: number;
  failedCount: number;
  failures: { instanceId: string; error: string }[];
}

export interface IGetWorkflowInstancesInput extends IPagedAndSortedResultRequest {
  entityType?: number;
  status?: number;
  entityId?: string;
  search?: string;
}

// ============================================================
// WorkflowTransition
// ============================================================
export interface IWorkflowTransition {
  id: string;
  workflowInstanceId: string;
  fromStepId: string;
  fromStepName?: string;
  toStepId?: string;
  toStepName?: string;
  action: number;
  actorUserId: number;
  actorUserName?: string;
  comment?: string;
  transitionDate: string;
  attachmentUrl?: string;
}

// ============================================================
// WorkflowDelegation
// ============================================================
export interface IWorkflowDelegation {
  id: string;
  delegatorUserId: number;
  delegateUserId: number;
  delegatorUserName?: string;
  delegateUserName?: string;
  startDate: string;
  endDate: string;
  reason?: string;
  isActive: boolean;
  entityType?: number;
  assignedRole?: string;
  isEffective: boolean;
  creationTime: string;
}

export interface ICreateWorkflowDelegation {
  delegateUserId: number;
  startDate: string;
  endDate: string;
  reason?: string;
  entityType?: number;
  assignedRole?: string;
}

export interface IGetWorkflowDelegationsInput extends IPagedAndSortedResultRequest {
  delegatorUserId?: number;
  delegateUserId?: number;
  isActive?: boolean;
  search?: string;
}

// ============================================================
// WorkflowDashboard
// ============================================================
export interface IWorkflowDashboard {
  totalActive: number;
  totalCompleted: number;
  totalRejected: number;
  totalOverdue: number;
  averageCompletionHours?: number;
  byEntityType: IWorkflowEntityTypeSummary[];
  byRole: IWorkflowRoleSummary[];
  myPendingCount: number;
}

export interface IWorkflowEntityTypeSummary {
  entityType: number;
  activeCount: number;
  completedCount: number;
  rejectedCount: number;
  overdueCount: number;
}

export interface IWorkflowRoleSummary {
  roleName: string;
  pendingCount: number;
  overdueCount: number;
}

export interface IWorkflowActivity {
  workflowInstanceId: string;
  entityType: number;
  entityId: string;
  workflowDefinitionName: string;
  action: number;
  actorUserName?: string;
  fromStepName?: string;
  toStepName?: string;
  comment?: string;
  transitionDate: string;
}
