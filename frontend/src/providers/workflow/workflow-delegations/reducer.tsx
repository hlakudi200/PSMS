import { handleActions } from "redux-actions";
import { INITIAL_STATE, IWorkflowDelegationStateContext } from "./context";
import { WorkflowDelegationActionEnums } from "./actions";

export const WorkflowDelegationReducer = handleActions<
  IWorkflowDelegationStateContext,
  IWorkflowDelegationStateContext
>(
  {
    [WorkflowDelegationActionEnums.getDelegationPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDelegationActionEnums.getDelegationSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDelegationActionEnums.getDelegationError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDelegationActionEnums.getDelegationsPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDelegationActionEnums.getDelegationsSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDelegationActionEnums.getDelegationsError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDelegationActionEnums.createDelegationPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDelegationActionEnums.createDelegationSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDelegationActionEnums.createDelegationError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDelegationActionEnums.revokeDelegationPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDelegationActionEnums.revokeDelegationSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDelegationActionEnums.revokeDelegationError]: (state, action) => ({
      ...state, ...action.payload,
    }),
  },
  INITIAL_STATE
);
