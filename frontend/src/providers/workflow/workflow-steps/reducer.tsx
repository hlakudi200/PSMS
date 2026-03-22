import { handleActions } from "redux-actions";
import { INITIAL_STATE, IWorkflowStepStateContext } from "./context";
import { WorkflowStepActionEnums } from "./actions";

export const WorkflowStepReducer = handleActions<
  IWorkflowStepStateContext,
  IWorkflowStepStateContext
>(
  {
    [WorkflowStepActionEnums.getStepPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowStepActionEnums.getStepSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowStepActionEnums.getStepError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowStepActionEnums.getStepsPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowStepActionEnums.getStepsSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowStepActionEnums.getStepsError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowStepActionEnums.createStepPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowStepActionEnums.createStepSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowStepActionEnums.createStepError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowStepActionEnums.updateStepPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowStepActionEnums.updateStepSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowStepActionEnums.updateStepError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowStepActionEnums.deleteStepPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowStepActionEnums.deleteStepSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowStepActionEnums.deleteStepError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowStepActionEnums.reorderStepsPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowStepActionEnums.reorderStepsSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowStepActionEnums.reorderStepsError]: (state, action) => ({
      ...state, ...action.payload,
    }),
  },
  INITIAL_STATE
);
