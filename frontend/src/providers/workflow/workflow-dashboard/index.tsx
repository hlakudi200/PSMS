"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  WorkflowDashboardActionContext,
  WorkflowDashboardStateContext,
} from "./context";
import { WorkflowDashboardReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getDashboardPending,
  getDashboardSuccess,
  getDashboardError,
  getActivityPending,
  getActivitySuccess,
  getActivityError,
} from "./actions";

export const WorkflowDashboardProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(WorkflowDashboardReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getDashboardAsync = async () => {
    dispatch(getDashboardPending());
    const endpoint = `/api/services/app/WorkflowDashboard/GetDashboard`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getDashboardSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getDashboardError());
      });
  };

  const getRecentActivityAsync = async (count: number = 20) => {
    dispatch(getActivityPending());
    const endpoint = `/api/services/app/WorkflowDashboard/GetRecentActivity?count=${count}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getActivitySuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getActivityError());
      });
  };

  return (
    <WorkflowDashboardStateContext.Provider value={state}>
      <WorkflowDashboardActionContext.Provider
        value={{
          getDashboardAsync,
          getRecentActivityAsync,
        }}
      >
        {children}
      </WorkflowDashboardActionContext.Provider>
    </WorkflowDashboardStateContext.Provider>
  );
};

export const useWorkflowDashboardState = () => {
  const context = useContext(WorkflowDashboardStateContext);
  if (!context) {
    throw new Error("useWorkflowDashboardState must be used within a WorkflowDashboardProvider");
  }
  return context;
};

export const useWorkflowDashboardActions = () => {
  const context = useContext(WorkflowDashboardActionContext);
  if (!context) {
    throw new Error("useWorkflowDashboardActions must be used within a WorkflowDashboardProvider");
  }
  return context;
};
