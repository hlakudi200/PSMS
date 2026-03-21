"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import { buildQueryParams } from "@/utils/query-params";
import {
  INITIAL_STATE,
  WorkflowDelegationActionContext,
  WorkflowDelegationStateContext,
} from "./context";
import type {
  ICreateWorkflowDelegation,
  IGetWorkflowDelegationsInput,
  IPagedAndSortedResultRequest,
} from "../shared/interfaces";
import { WorkflowDelegationReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getDelegationPending,
  getDelegationSuccess,
  getDelegationError,
  getDelegationsPending,
  getDelegationsSuccess,
  getDelegationsError,
  createDelegationPending,
  createDelegationSuccess,
  createDelegationError,
  revokeDelegationPending,
  revokeDelegationSuccess,
  revokeDelegationError,
} from "./actions";

export const WorkflowDelegationProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(WorkflowDelegationReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getDelegationPending());
    const endpoint = `/api/services/app/WorkflowDelegation/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getDelegationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getDelegationError());
      });
  };

  const getAllAsync = async (input?: IGetWorkflowDelegationsInput) => {
    dispatch(getDelegationsPending());
    const params = buildQueryParams(input as Record<string, unknown>);
    const endpoint = `/api/services/app/WorkflowDelegation/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getDelegationsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getDelegationsError());
      });
  };

  const getMyDelegationsAsync = async (input?: IPagedAndSortedResultRequest) => {
    dispatch(getDelegationsPending());
    const params = buildQueryParams(input as Record<string, unknown>);
    const endpoint = `/api/services/app/WorkflowDelegation/GetMyDelegations?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getDelegationsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getDelegationsError());
      });
  };

  const createAsync = async (input: ICreateWorkflowDelegation) => {
    dispatch(createDelegationPending());
    const endpoint = `/api/services/app/WorkflowDelegation/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createDelegationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createDelegationError());
      });
  };

  const revokeAsync = async (id: string) => {
    dispatch(revokeDelegationPending());
    const endpoint = `/api/services/app/WorkflowDelegation/Revoke?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(revokeDelegationSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(revokeDelegationError());
      });
  };

  return (
    <WorkflowDelegationStateContext.Provider value={state}>
      <WorkflowDelegationActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getMyDelegationsAsync,
          createAsync,
          revokeAsync,
        }}
      >
        {children}
      </WorkflowDelegationActionContext.Provider>
    </WorkflowDelegationStateContext.Provider>
  );
};

export const useWorkflowDelegationState = () => {
  const context = useContext(WorkflowDelegationStateContext);
  if (!context) {
    throw new Error("useWorkflowDelegationState must be used within a WorkflowDelegationProvider");
  }
  return context;
};

export const useWorkflowDelegationActions = () => {
  const context = useContext(WorkflowDelegationActionContext);
  if (!context) {
    throw new Error("useWorkflowDelegationActions must be used within a WorkflowDelegationProvider");
  }
  return context;
};
