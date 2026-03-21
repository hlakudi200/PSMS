import { createAction } from "redux-actions";
import { IWorkflowInstanceStateContext } from "./context";
import type {
  IWorkflowInstance,
  IWorkflowInstanceList,
  IWorkflowTransition,
  IBatchAdvanceResult,
  IPagedResult,
  IListResult,
} from "../shared/interfaces";

export enum WorkflowInstanceActionEnums {
  getInstancePending = "GET_WORKFLOW_INSTANCE_PENDING",
  getInstanceSuccess = "GET_WORKFLOW_INSTANCE_SUCCESS",
  getInstanceError = "GET_WORKFLOW_INSTANCE_ERROR",

  getInstancesPending = "GET_WORKFLOW_INSTANCES_PENDING",
  getInstancesSuccess = "GET_WORKFLOW_INSTANCES_SUCCESS",
  getInstancesError = "GET_WORKFLOW_INSTANCES_ERROR",

  startPending = "START_WORKFLOW_PENDING",
  startSuccess = "START_WORKFLOW_SUCCESS",
  startError = "START_WORKFLOW_ERROR",

  advancePending = "ADVANCE_WORKFLOW_PENDING",
  advanceSuccess = "ADVANCE_WORKFLOW_SUCCESS",
  advanceError = "ADVANCE_WORKFLOW_ERROR",

  cancelPending = "CANCEL_WORKFLOW_PENDING",
  cancelSuccess = "CANCEL_WORKFLOW_SUCCESS",
  cancelError = "CANCEL_WORKFLOW_ERROR",

  recallPending = "RECALL_WORKFLOW_PENDING",
  recallSuccess = "RECALL_WORKFLOW_SUCCESS",
  recallError = "RECALL_WORKFLOW_ERROR",

  batchAdvancePending = "BATCH_ADVANCE_WORKFLOW_PENDING",
  batchAdvanceSuccess = "BATCH_ADVANCE_WORKFLOW_SUCCESS",
  batchAdvanceError = "BATCH_ADVANCE_WORKFLOW_ERROR",

  getHistoryPending = "GET_WORKFLOW_HISTORY_PENDING",
  getHistorySuccess = "GET_WORKFLOW_HISTORY_SUCCESS",
  getHistoryError = "GET_WORKFLOW_HISTORY_ERROR",
}

// Get Single
export const getInstancePending = createAction<IWorkflowInstanceStateContext>(
  WorkflowInstanceActionEnums.getInstancePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getInstanceSuccess = createAction<IWorkflowInstanceStateContext, IWorkflowInstance>(
  WorkflowInstanceActionEnums.getInstanceSuccess,
  (instance: IWorkflowInstance) => ({
    isPending: false, isSuccess: true, isError: false, instance,
  })
);
export const getInstanceError = createAction<IWorkflowInstanceStateContext>(
  WorkflowInstanceActionEnums.getInstanceError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All
export const getInstancesPending = createAction<IWorkflowInstanceStateContext>(
  WorkflowInstanceActionEnums.getInstancesPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getInstancesSuccess = createAction<IWorkflowInstanceStateContext, IPagedResult<IWorkflowInstanceList>>(
  WorkflowInstanceActionEnums.getInstancesSuccess,
  (result: IPagedResult<IWorkflowInstanceList>) => ({
    isPending: false, isSuccess: true, isError: false,
    instances: result.items, totalCount: result.totalCount,
  })
);
export const getInstancesError = createAction<IWorkflowInstanceStateContext>(
  WorkflowInstanceActionEnums.getInstancesError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Start
export const startPending = createAction<IWorkflowInstanceStateContext>(
  WorkflowInstanceActionEnums.startPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const startSuccess = createAction<IWorkflowInstanceStateContext, IWorkflowInstance>(
  WorkflowInstanceActionEnums.startSuccess,
  (instance: IWorkflowInstance) => ({
    isPending: false, isSuccess: true, isError: false, instance,
  })
);
export const startError = createAction<IWorkflowInstanceStateContext>(
  WorkflowInstanceActionEnums.startError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Advance
export const advancePending = createAction<IWorkflowInstanceStateContext>(
  WorkflowInstanceActionEnums.advancePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const advanceSuccess = createAction<IWorkflowInstanceStateContext, IWorkflowInstance>(
  WorkflowInstanceActionEnums.advanceSuccess,
  (instance: IWorkflowInstance) => ({
    isPending: false, isSuccess: true, isError: false, instance,
  })
);
export const advanceError = createAction<IWorkflowInstanceStateContext>(
  WorkflowInstanceActionEnums.advanceError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Cancel
export const cancelPending = createAction<IWorkflowInstanceStateContext>(
  WorkflowInstanceActionEnums.cancelPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const cancelSuccess = createAction<IWorkflowInstanceStateContext, IWorkflowInstance>(
  WorkflowInstanceActionEnums.cancelSuccess,
  (instance: IWorkflowInstance) => ({
    isPending: false, isSuccess: true, isError: false, instance,
  })
);
export const cancelError = createAction<IWorkflowInstanceStateContext>(
  WorkflowInstanceActionEnums.cancelError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Recall
export const recallPending = createAction<IWorkflowInstanceStateContext>(
  WorkflowInstanceActionEnums.recallPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const recallSuccess = createAction<IWorkflowInstanceStateContext, IWorkflowInstance>(
  WorkflowInstanceActionEnums.recallSuccess,
  (instance: IWorkflowInstance) => ({
    isPending: false, isSuccess: true, isError: false, instance,
  })
);
export const recallError = createAction<IWorkflowInstanceStateContext>(
  WorkflowInstanceActionEnums.recallError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Batch Advance
export const batchAdvancePending = createAction<IWorkflowInstanceStateContext>(
  WorkflowInstanceActionEnums.batchAdvancePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const batchAdvanceSuccess = createAction<IWorkflowInstanceStateContext, IBatchAdvanceResult>(
  WorkflowInstanceActionEnums.batchAdvanceSuccess,
  (batchResult: IBatchAdvanceResult) => ({
    isPending: false, isSuccess: true, isError: false, batchResult,
  })
);
export const batchAdvanceError = createAction<IWorkflowInstanceStateContext>(
  WorkflowInstanceActionEnums.batchAdvanceError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get History
export const getHistoryPending = createAction<IWorkflowInstanceStateContext>(
  WorkflowInstanceActionEnums.getHistoryPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getHistorySuccess = createAction<IWorkflowInstanceStateContext, IListResult<IWorkflowTransition>>(
  WorkflowInstanceActionEnums.getHistorySuccess,
  (result: IListResult<IWorkflowTransition>) => ({
    isPending: false, isSuccess: true, isError: false,
    history: result.items,
  })
);
export const getHistoryError = createAction<IWorkflowInstanceStateContext>(
  WorkflowInstanceActionEnums.getHistoryError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
