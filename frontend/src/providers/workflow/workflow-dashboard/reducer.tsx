import { handleActions } from "redux-actions";
import { INITIAL_STATE, IWorkflowDashboardStateContext } from "./context";
import { WorkflowDashboardActionEnums } from "./actions";

export const WorkflowDashboardReducer = handleActions<
  IWorkflowDashboardStateContext,
  IWorkflowDashboardStateContext
>(
  {
    [WorkflowDashboardActionEnums.getDashboardPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDashboardActionEnums.getDashboardSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDashboardActionEnums.getDashboardError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDashboardActionEnums.getActivityPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDashboardActionEnums.getActivitySuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDashboardActionEnums.getActivityError]: (state, action) => ({
      ...state, ...action.payload,
    }),
  },
  INITIAL_STATE
);
