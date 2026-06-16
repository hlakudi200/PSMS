"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import { buildQueryParams } from "@/utils/query-params";
import {
  INITIAL_STATE,
  WorkflowInstanceActionContext,
  WorkflowInstanceStateContext,
} from "./context";
import type {
  IStartWorkflow,
  IAdvanceWorkflow,
  IBatchAdvance,
  IGetWorkflowInstancesInput,
  IPagedAndSortedResultRequest,
} from "../shared/interfaces";
import { WorkflowInstanceReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getInstancePending,
  getInstanceSuccess,
  getInstanceError,
  getInstancesPending,
  getInstancesSuccess,
  getInstancesError,
  startPending,
  startSuccess,
  startError,
  advancePending,
  advanceSuccess,
  advanceError,
  cancelPending,
  cancelSuccess,
  cancelError,
  recallPending,
  recallSuccess,
  recallError,
  batchAdvancePending,
  batchAdvanceSuccess,
  batchAdvanceError,
  getHistoryPending,
  getHistorySuccess,
  getHistoryError,
} from "./actions";

export const WorkflowInstanceProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(WorkflowInstanceReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getInstancePending());
    const endpoint = `/api/services/app/WorkflowInstance/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getInstanceSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getInstanceError());
      });
  };

  const getByEntityAsync = async (entityType: number, entityId: string) => {
    dispatch(getInstancePending());
    const endpoint = `/api/services/app/WorkflowInstance/GetByEntity?entityType=${entityType}&entityId=${entityId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getInstanceSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getInstanceError());
      });
  };

  const getAllAsync = async (input?: IGetWorkflowInstancesInput) => {
    dispatch(getInstancesPending());
    const params = buildQueryParams(input as Record<string, unknown>);
    const endpoint = `/api/services/app/WorkflowInstance/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getInstancesSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getInstancesError());
      });
  };

  const startAsync = async (input: IStartWorkflow) => {
    dispatch(startPending());
    const endpoint = `/api/services/app/WorkflowInstance/Start`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(startSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(startError());
      });
  };

  const advanceAsync = async (id: string, input: IAdvanceWorkflow) => {
    dispatch(advancePending());
    const endpoint = `/api/services/app/WorkflowInstance/Advance?instanceId=${id}`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(advanceSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(advanceError());
      });
  };

  const cancelAsync = async (id: string, comment?: string) => {
    dispatch(cancelPending());
    const endpoint = `/api/services/app/WorkflowInstance/Cancel?instanceId=${id}`;
    await instance
      .post(endpoint, { comment })
      .then((response) => {
        dispatch(cancelSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(cancelError());
      });
  };

  const recallAsync = async (id: string, comment?: string) => {
    dispatch(recallPending());
    const endpoint = `/api/services/app/WorkflowInstance/Recall?instanceId=${id}`;
    await instance
      .post(endpoint, { comment })
      .then((response) => {
        dispatch(recallSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(recallError());
      });
  };

  const batchAdvanceAsync = async (input: IBatchAdvance) => {
    dispatch(batchAdvancePending());
    const endpoint = `/api/services/app/WorkflowInstance/BatchAdvance`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(batchAdvanceSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(batchAdvanceError());
      });
  };

  const getHistoryAsync = async (instanceId: string) => {
    dispatch(getHistoryPending());
    const endpoint = `/api/services/app/WorkflowInstance/GetHistory?instanceId=${instanceId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getHistorySuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getHistoryError());
      });
  };

  const getPendingForRoleAsync = async (roleName: string, input?: IPagedAndSortedResultRequest) => {
    dispatch(getInstancesPending());
    const params = buildQueryParams(input as Record<string, unknown>);
    const endpoint = `/api/services/app/WorkflowInstance/GetPendingForRole?roleName=${roleName}&${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getInstancesSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getInstancesError());
      });
  };

  // WF-03: the current user's actionable "my tasks" — server scopes by the
  // caller's user/role assignment, so no role param is passed.
  const getMyPendingAsync = async (input?: IPagedAndSortedResultRequest) => {
    dispatch(getInstancesPending());
    const params = buildQueryParams(input as Record<string, unknown>);
    const endpoint = `/api/services/app/WorkflowInstance/GetMyPending?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getInstancesSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getInstancesError());
      });
  };

  const getOverdueAsync = async (input?: IPagedAndSortedResultRequest) => {
    dispatch(getInstancesPending());
    const params = buildQueryParams(input as Record<string, unknown>);
    const endpoint = `/api/services/app/WorkflowInstance/GetOverdue?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getInstancesSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getInstancesError());
      });
  };

  return (
    <WorkflowInstanceStateContext.Provider value={state}>
      <WorkflowInstanceActionContext.Provider
        value={{
          getAsync,
          getByEntityAsync,
          getAllAsync,
          startAsync,
          advanceAsync,
          cancelAsync,
          recallAsync,
          batchAdvanceAsync,
          getHistoryAsync,
          getPendingForRoleAsync,
          getMyPendingAsync,
          getOverdueAsync,
        }}
      >
        {children}
      </WorkflowInstanceActionContext.Provider>
    </WorkflowInstanceStateContext.Provider>
  );
};

export const useWorkflowInstanceState = () => {
  const context = useContext(WorkflowInstanceStateContext);
  if (!context) {
    throw new Error("useWorkflowInstanceState must be used within a WorkflowInstanceProvider");
  }
  return context;
};

export const useWorkflowInstanceActions = () => {
  const context = useContext(WorkflowInstanceActionContext);
  if (!context) {
    throw new Error("useWorkflowInstanceActions must be used within a WorkflowInstanceProvider");
  }
  return context;
};
