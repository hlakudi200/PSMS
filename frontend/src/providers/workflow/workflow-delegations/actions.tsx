import { createAction } from "redux-actions";
import { IWorkflowDelegationStateContext } from "./context";
import type { IWorkflowDelegation, IPagedResult } from "../shared/interfaces";

export enum WorkflowDelegationActionEnums {
  getDelegationPending = "GET_WORKFLOW_DELEGATION_PENDING",
  getDelegationSuccess = "GET_WORKFLOW_DELEGATION_SUCCESS",
  getDelegationError = "GET_WORKFLOW_DELEGATION_ERROR",

  getDelegationsPending = "GET_WORKFLOW_DELEGATIONS_PENDING",
  getDelegationsSuccess = "GET_WORKFLOW_DELEGATIONS_SUCCESS",
  getDelegationsError = "GET_WORKFLOW_DELEGATIONS_ERROR",

  createDelegationPending = "CREATE_WORKFLOW_DELEGATION_PENDING",
  createDelegationSuccess = "CREATE_WORKFLOW_DELEGATION_SUCCESS",
  createDelegationError = "CREATE_WORKFLOW_DELEGATION_ERROR",

  revokeDelegationPending = "REVOKE_WORKFLOW_DELEGATION_PENDING",
  revokeDelegationSuccess = "REVOKE_WORKFLOW_DELEGATION_SUCCESS",
  revokeDelegationError = "REVOKE_WORKFLOW_DELEGATION_ERROR",
}

// Get Single
export const getDelegationPending = createAction<IWorkflowDelegationStateContext>(
  WorkflowDelegationActionEnums.getDelegationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getDelegationSuccess = createAction<IWorkflowDelegationStateContext, IWorkflowDelegation>(
  WorkflowDelegationActionEnums.getDelegationSuccess,
  (delegation: IWorkflowDelegation) => ({
    isPending: false, isSuccess: true, isError: false, delegation,
  })
);
export const getDelegationError = createAction<IWorkflowDelegationStateContext>(
  WorkflowDelegationActionEnums.getDelegationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All
export const getDelegationsPending = createAction<IWorkflowDelegationStateContext>(
  WorkflowDelegationActionEnums.getDelegationsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getDelegationsSuccess = createAction<IWorkflowDelegationStateContext, IPagedResult<IWorkflowDelegation>>(
  WorkflowDelegationActionEnums.getDelegationsSuccess,
  (result: IPagedResult<IWorkflowDelegation>) => ({
    isPending: false, isSuccess: true, isError: false,
    delegations: result.items, totalCount: result.totalCount,
  })
);
export const getDelegationsError = createAction<IWorkflowDelegationStateContext>(
  WorkflowDelegationActionEnums.getDelegationsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create
export const createDelegationPending = createAction<IWorkflowDelegationStateContext>(
  WorkflowDelegationActionEnums.createDelegationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const createDelegationSuccess = createAction<IWorkflowDelegationStateContext, IWorkflowDelegation>(
  WorkflowDelegationActionEnums.createDelegationSuccess,
  (delegation: IWorkflowDelegation) => ({
    isPending: false, isSuccess: true, isError: false, delegation,
  })
);
export const createDelegationError = createAction<IWorkflowDelegationStateContext>(
  WorkflowDelegationActionEnums.createDelegationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Revoke
export const revokeDelegationPending = createAction<IWorkflowDelegationStateContext>(
  WorkflowDelegationActionEnums.revokeDelegationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const revokeDelegationSuccess = createAction<IWorkflowDelegationStateContext>(
  WorkflowDelegationActionEnums.revokeDelegationSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const revokeDelegationError = createAction<IWorkflowDelegationStateContext>(
  WorkflowDelegationActionEnums.revokeDelegationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
