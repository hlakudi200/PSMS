import { createAction } from "redux-actions";
import { IWorkflowDefinitionStateContext } from "./context";
import type { IWorkflowDefinition, IWorkflowDefinitionList, IPagedResult } from "../shared/interfaces";

export enum WorkflowDefinitionActionEnums {
  getDefinitionPending = "GET_WORKFLOW_DEFINITION_PENDING",
  getDefinitionSuccess = "GET_WORKFLOW_DEFINITION_SUCCESS",
  getDefinitionError = "GET_WORKFLOW_DEFINITION_ERROR",

  getDefinitionsPending = "GET_WORKFLOW_DEFINITIONS_PENDING",
  getDefinitionsSuccess = "GET_WORKFLOW_DEFINITIONS_SUCCESS",
  getDefinitionsError = "GET_WORKFLOW_DEFINITIONS_ERROR",

  createDefinitionPending = "CREATE_WORKFLOW_DEFINITION_PENDING",
  createDefinitionSuccess = "CREATE_WORKFLOW_DEFINITION_SUCCESS",
  createDefinitionError = "CREATE_WORKFLOW_DEFINITION_ERROR",

  updateDefinitionPending = "UPDATE_WORKFLOW_DEFINITION_PENDING",
  updateDefinitionSuccess = "UPDATE_WORKFLOW_DEFINITION_SUCCESS",
  updateDefinitionError = "UPDATE_WORKFLOW_DEFINITION_ERROR",

  deleteDefinitionPending = "DELETE_WORKFLOW_DEFINITION_PENDING",
  deleteDefinitionSuccess = "DELETE_WORKFLOW_DEFINITION_SUCCESS",
  deleteDefinitionError = "DELETE_WORKFLOW_DEFINITION_ERROR",

  activatePending = "ACTIVATE_WORKFLOW_DEFINITION_PENDING",
  activateSuccess = "ACTIVATE_WORKFLOW_DEFINITION_SUCCESS",
  activateError = "ACTIVATE_WORKFLOW_DEFINITION_ERROR",

  deactivatePending = "DEACTIVATE_WORKFLOW_DEFINITION_PENDING",
  deactivateSuccess = "DEACTIVATE_WORKFLOW_DEFINITION_SUCCESS",
  deactivateError = "DEACTIVATE_WORKFLOW_DEFINITION_ERROR",
}

// Get Single
export const getDefinitionPending = createAction<IWorkflowDefinitionStateContext>(
  WorkflowDefinitionActionEnums.getDefinitionPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getDefinitionSuccess = createAction<IWorkflowDefinitionStateContext, IWorkflowDefinition>(
  WorkflowDefinitionActionEnums.getDefinitionSuccess,
  (definition: IWorkflowDefinition) => ({
    isPending: false, isSuccess: true, isError: false, definition,
  })
);
export const getDefinitionError = createAction<IWorkflowDefinitionStateContext>(
  WorkflowDefinitionActionEnums.getDefinitionError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All
export const getDefinitionsPending = createAction<IWorkflowDefinitionStateContext>(
  WorkflowDefinitionActionEnums.getDefinitionsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getDefinitionsSuccess = createAction<IWorkflowDefinitionStateContext, IPagedResult<IWorkflowDefinitionList>>(
  WorkflowDefinitionActionEnums.getDefinitionsSuccess,
  (result: IPagedResult<IWorkflowDefinitionList>) => ({
    isPending: false, isSuccess: true, isError: false,
    definitions: result.items, totalCount: result.totalCount,
  })
);
export const getDefinitionsError = createAction<IWorkflowDefinitionStateContext>(
  WorkflowDefinitionActionEnums.getDefinitionsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create
export const createDefinitionPending = createAction<IWorkflowDefinitionStateContext>(
  WorkflowDefinitionActionEnums.createDefinitionPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const createDefinitionSuccess = createAction<IWorkflowDefinitionStateContext, IWorkflowDefinition>(
  WorkflowDefinitionActionEnums.createDefinitionSuccess,
  (definition: IWorkflowDefinition) => ({
    isPending: false, isSuccess: true, isError: false, definition,
  })
);
export const createDefinitionError = createAction<IWorkflowDefinitionStateContext>(
  WorkflowDefinitionActionEnums.createDefinitionError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update
export const updateDefinitionPending = createAction<IWorkflowDefinitionStateContext>(
  WorkflowDefinitionActionEnums.updateDefinitionPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const updateDefinitionSuccess = createAction<IWorkflowDefinitionStateContext, IWorkflowDefinition>(
  WorkflowDefinitionActionEnums.updateDefinitionSuccess,
  (definition: IWorkflowDefinition) => ({
    isPending: false, isSuccess: true, isError: false, definition,
  })
);
export const updateDefinitionError = createAction<IWorkflowDefinitionStateContext>(
  WorkflowDefinitionActionEnums.updateDefinitionError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete
export const deleteDefinitionPending = createAction<IWorkflowDefinitionStateContext>(
  WorkflowDefinitionActionEnums.deleteDefinitionPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const deleteDefinitionSuccess = createAction<IWorkflowDefinitionStateContext>(
  WorkflowDefinitionActionEnums.deleteDefinitionSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const deleteDefinitionError = createAction<IWorkflowDefinitionStateContext>(
  WorkflowDefinitionActionEnums.deleteDefinitionError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Activate
export const activatePending = createAction<IWorkflowDefinitionStateContext>(
  WorkflowDefinitionActionEnums.activatePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const activateSuccess = createAction<IWorkflowDefinitionStateContext>(
  WorkflowDefinitionActionEnums.activateSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const activateError = createAction<IWorkflowDefinitionStateContext>(
  WorkflowDefinitionActionEnums.activateError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Deactivate
export const deactivatePending = createAction<IWorkflowDefinitionStateContext>(
  WorkflowDefinitionActionEnums.deactivatePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const deactivateSuccess = createAction<IWorkflowDefinitionStateContext>(
  WorkflowDefinitionActionEnums.deactivateSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const deactivateError = createAction<IWorkflowDefinitionStateContext>(
  WorkflowDefinitionActionEnums.deactivateError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
