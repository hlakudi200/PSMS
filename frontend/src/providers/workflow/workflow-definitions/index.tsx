"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import { buildQueryParams } from "@/utils/query-params";
import {
  INITIAL_STATE,
  WorkflowDefinitionActionContext,
  WorkflowDefinitionStateContext,
} from "./context";
import type {
  ICloneWorkflowDefinition,
  IWorkflowDefinition,
  ICreateWorkflowDefinition,
  IUpdateWorkflowDefinition,
  IGetWorkflowDefinitionsInput,
} from "../shared/interfaces";
import { WorkflowDefinitionReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getDefinitionPending,
  getDefinitionSuccess,
  getDefinitionError,
  getDefinitionsPending,
  getDefinitionsSuccess,
  getDefinitionsError,
  createDefinitionPending,
  createDefinitionSuccess,
  createDefinitionError,
  updateDefinitionPending,
  updateDefinitionSuccess,
  updateDefinitionError,
  deleteDefinitionPending,
  deleteDefinitionSuccess,
  deleteDefinitionError,
  activatePending,
  activateSuccess,
  activateError,
  deactivatePending,
  deactivateSuccess,
  deactivateError,
} from "./actions";

export const WorkflowDefinitionProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(WorkflowDefinitionReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getDefinitionPending());
    const endpoint = `/api/services/app/WorkflowDefinition/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getDefinitionSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getDefinitionError());
      });
  };

  const getAllAsync = async (input?: IGetWorkflowDefinitionsInput) => {
    dispatch(getDefinitionsPending());
    const params = buildQueryParams(input as Record<string, unknown>);
    const endpoint = `/api/services/app/WorkflowDefinition/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getDefinitionsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getDefinitionsError());
      });
  };

  const createAsync = async (input: ICreateWorkflowDefinition) => {
    dispatch(createDefinitionPending());
    const endpoint = `/api/services/app/WorkflowDefinition/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createDefinitionSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createDefinitionError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateWorkflowDefinition) => {
    dispatch(updateDefinitionPending());
    const endpoint = `/api/services/app/WorkflowDefinition/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateDefinitionSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateDefinitionError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteDefinitionPending());
    const endpoint = `/api/services/app/WorkflowDefinition/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteDefinitionSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteDefinitionError());
      });
  };

  const activateAsync = async (id: string) => {
    dispatch(activatePending());
    const endpoint = `/api/services/app/WorkflowDefinition/Activate?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(activateSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(activateError());
      });
  };

  const deactivateAsync = async (id: string) => {
    dispatch(deactivatePending());
    const endpoint = `/api/services/app/WorkflowDefinition/Deactivate?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(deactivateSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deactivateError());
      });
  };

  // WF-33: clone as a new inactive version (steps of a running definition are locked).
  const cloneAsync = async (id: string, input?: ICloneWorkflowDefinition): Promise<IWorkflowDefinition | undefined> => {
    dispatch(createDefinitionPending());
    const endpoint = `/api/services/app/WorkflowDefinition/Clone?id=${id}`;
    try {
      const response = await instance.post(endpoint, input ?? {});
      dispatch(createDefinitionSuccess(response.data.result));
      return response.data.result as IWorkflowDefinition;
    } catch (error) {
      console.error(error);
      dispatch(createDefinitionError());
      return undefined;
    }
  };

  return (
    <WorkflowDefinitionStateContext.Provider value={state}>
      <WorkflowDefinitionActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          activateAsync,
          deactivateAsync,
          cloneAsync,
        }}
      >
        {children}
      </WorkflowDefinitionActionContext.Provider>
    </WorkflowDefinitionStateContext.Provider>
  );
};

export const useWorkflowDefinitionState = () => {
  const context = useContext(WorkflowDefinitionStateContext);
  if (!context) {
    throw new Error("useWorkflowDefinitionState must be used within a WorkflowDefinitionProvider");
  }
  return context;
};

export const useWorkflowDefinitionActions = () => {
  const context = useContext(WorkflowDefinitionActionContext);
  if (!context) {
    throw new Error("useWorkflowDefinitionActions must be used within a WorkflowDefinitionProvider");
  }
  return context;
};
