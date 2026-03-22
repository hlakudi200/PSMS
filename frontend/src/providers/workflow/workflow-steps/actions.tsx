import { createAction } from "redux-actions";
import { IWorkflowStepStateContext } from "./context";
import type { IWorkflowStep, IListResult } from "../shared/interfaces";

export enum WorkflowStepActionEnums {
  getStepPending = "GET_WORKFLOW_STEP_PENDING",
  getStepSuccess = "GET_WORKFLOW_STEP_SUCCESS",
  getStepError = "GET_WORKFLOW_STEP_ERROR",

  getStepsPending = "GET_WORKFLOW_STEPS_PENDING",
  getStepsSuccess = "GET_WORKFLOW_STEPS_SUCCESS",
  getStepsError = "GET_WORKFLOW_STEPS_ERROR",

  createStepPending = "CREATE_WORKFLOW_STEP_PENDING",
  createStepSuccess = "CREATE_WORKFLOW_STEP_SUCCESS",
  createStepError = "CREATE_WORKFLOW_STEP_ERROR",

  updateStepPending = "UPDATE_WORKFLOW_STEP_PENDING",
  updateStepSuccess = "UPDATE_WORKFLOW_STEP_SUCCESS",
  updateStepError = "UPDATE_WORKFLOW_STEP_ERROR",

  deleteStepPending = "DELETE_WORKFLOW_STEP_PENDING",
  deleteStepSuccess = "DELETE_WORKFLOW_STEP_SUCCESS",
  deleteStepError = "DELETE_WORKFLOW_STEP_ERROR",

  reorderStepsPending = "REORDER_WORKFLOW_STEPS_PENDING",
  reorderStepsSuccess = "REORDER_WORKFLOW_STEPS_SUCCESS",
  reorderStepsError = "REORDER_WORKFLOW_STEPS_ERROR",
}

// Get Single
export const getStepPending = createAction<IWorkflowStepStateContext>(
  WorkflowStepActionEnums.getStepPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getStepSuccess = createAction<IWorkflowStepStateContext, IWorkflowStep>(
  WorkflowStepActionEnums.getStepSuccess,
  (step: IWorkflowStep) => ({
    isPending: false, isSuccess: true, isError: false, step,
  })
);
export const getStepError = createAction<IWorkflowStepStateContext>(
  WorkflowStepActionEnums.getStepError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Definition
export const getStepsPending = createAction<IWorkflowStepStateContext>(
  WorkflowStepActionEnums.getStepsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getStepsSuccess = createAction<IWorkflowStepStateContext, IListResult<IWorkflowStep>>(
  WorkflowStepActionEnums.getStepsSuccess,
  (result: IListResult<IWorkflowStep>) => ({
    isPending: false, isSuccess: true, isError: false,
    steps: result.items,
  })
);
export const getStepsError = createAction<IWorkflowStepStateContext>(
  WorkflowStepActionEnums.getStepsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create
export const createStepPending = createAction<IWorkflowStepStateContext>(
  WorkflowStepActionEnums.createStepPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const createStepSuccess = createAction<IWorkflowStepStateContext, IWorkflowStep>(
  WorkflowStepActionEnums.createStepSuccess,
  (step: IWorkflowStep) => ({
    isPending: false, isSuccess: true, isError: false, step,
  })
);
export const createStepError = createAction<IWorkflowStepStateContext>(
  WorkflowStepActionEnums.createStepError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update
export const updateStepPending = createAction<IWorkflowStepStateContext>(
  WorkflowStepActionEnums.updateStepPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const updateStepSuccess = createAction<IWorkflowStepStateContext, IWorkflowStep>(
  WorkflowStepActionEnums.updateStepSuccess,
  (step: IWorkflowStep) => ({
    isPending: false, isSuccess: true, isError: false, step,
  })
);
export const updateStepError = createAction<IWorkflowStepStateContext>(
  WorkflowStepActionEnums.updateStepError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete
export const deleteStepPending = createAction<IWorkflowStepStateContext>(
  WorkflowStepActionEnums.deleteStepPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const deleteStepSuccess = createAction<IWorkflowStepStateContext>(
  WorkflowStepActionEnums.deleteStepSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const deleteStepError = createAction<IWorkflowStepStateContext>(
  WorkflowStepActionEnums.deleteStepError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Reorder
export const reorderStepsPending = createAction<IWorkflowStepStateContext>(
  WorkflowStepActionEnums.reorderStepsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const reorderStepsSuccess = createAction<IWorkflowStepStateContext, IListResult<IWorkflowStep>>(
  WorkflowStepActionEnums.reorderStepsSuccess,
  (result: IListResult<IWorkflowStep>) => ({
    isPending: false, isSuccess: true, isError: false,
    steps: result.items,
  })
);
export const reorderStepsError = createAction<IWorkflowStepStateContext>(
  WorkflowStepActionEnums.reorderStepsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
