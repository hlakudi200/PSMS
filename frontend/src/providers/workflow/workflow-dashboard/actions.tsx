import { createAction } from "redux-actions";
import { IWorkflowDashboardStateContext } from "./context";
import type { IWorkflowDashboard, IWorkflowActivity } from "../shared/interfaces";

export enum WorkflowDashboardActionEnums {
  getDashboardPending = "GET_WORKFLOW_DASHBOARD_PENDING",
  getDashboardSuccess = "GET_WORKFLOW_DASHBOARD_SUCCESS",
  getDashboardError = "GET_WORKFLOW_DASHBOARD_ERROR",

  getActivityPending = "GET_WORKFLOW_ACTIVITY_PENDING",
  getActivitySuccess = "GET_WORKFLOW_ACTIVITY_SUCCESS",
  getActivityError = "GET_WORKFLOW_ACTIVITY_ERROR",
}

// Get Dashboard
export const getDashboardPending = createAction<IWorkflowDashboardStateContext>(
  WorkflowDashboardActionEnums.getDashboardPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getDashboardSuccess = createAction<IWorkflowDashboardStateContext, IWorkflowDashboard>(
  WorkflowDashboardActionEnums.getDashboardSuccess,
  (dashboard: IWorkflowDashboard) => ({
    isPending: false, isSuccess: true, isError: false, dashboard,
  })
);
export const getDashboardError = createAction<IWorkflowDashboardStateContext>(
  WorkflowDashboardActionEnums.getDashboardError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Recent Activity
export const getActivityPending = createAction<IWorkflowDashboardStateContext>(
  WorkflowDashboardActionEnums.getActivityPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getActivitySuccess = createAction<IWorkflowDashboardStateContext, IWorkflowActivity[]>(
  WorkflowDashboardActionEnums.getActivitySuccess,
  (recentActivity: IWorkflowActivity[]) => ({
    isPending: false, isSuccess: true, isError: false, recentActivity,
  })
);
export const getActivityError = createAction<IWorkflowDashboardStateContext>(
  WorkflowDashboardActionEnums.getActivityError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
