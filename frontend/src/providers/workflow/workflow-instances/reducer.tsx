import { handleActions } from "redux-actions";
import { INITIAL_STATE, IWorkflowInstanceStateContext } from "./context";
import { WorkflowInstanceActionEnums } from "./actions";

export const WorkflowInstanceReducer = handleActions<
  IWorkflowInstanceStateContext,
  IWorkflowInstanceStateContext
>(
  {
    [WorkflowInstanceActionEnums.getInstancePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.getInstanceSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.getInstanceError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.getInstancesPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.getInstancesSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.getInstancesError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.startPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.startSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.startError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.advancePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.advanceSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.advanceError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.cancelPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.cancelSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.cancelError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.recallPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.recallSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.recallError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.batchAdvancePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.batchAdvanceSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.batchAdvanceError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.getHistoryPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.getHistorySuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowInstanceActionEnums.getHistoryError]: (state, action) => ({
      ...state, ...action.payload,
    }),
  },
  INITIAL_STATE
);
