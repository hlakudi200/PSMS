"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  WorkflowStepActionContext,
  WorkflowStepStateContext,
} from "./context";
import type {
  ICreateWorkflowStep,
  IUpdateWorkflowStep,
  IReorderWorkflowSteps,
} from "../shared/interfaces";
import { WorkflowStepReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getStepPending,
  getStepSuccess,
  getStepError,
  getStepsPending,
  getStepsSuccess,
  getStepsError,
  createStepPending,
  createStepSuccess,
  createStepError,
  updateStepPending,
  updateStepSuccess,
  updateStepError,
  deleteStepPending,
  deleteStepSuccess,
  deleteStepError,
  reorderStepsPending,
  reorderStepsSuccess,
  reorderStepsError,
} from "./actions";

export const WorkflowStepProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(WorkflowStepReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getStepPending());
    const endpoint = `/api/services/app/WorkflowStep/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getStepSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getStepError());
      });
  };

  const getByDefinitionAsync = async (definitionId: string) => {
    dispatch(getStepsPending());
    const endpoint = `/api/services/app/WorkflowStep/GetByDefinition?definitionId=${definitionId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getStepsSuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getStepsError());
      });
  };

  const createAsync = async (input: ICreateWorkflowStep) => {
    dispatch(createStepPending());
    const endpoint = `/api/services/app/WorkflowStep/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createStepSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createStepError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateWorkflowStep) => {
    dispatch(updateStepPending());
    const endpoint = `/api/services/app/WorkflowStep/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateStepSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateStepError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteStepPending());
    const endpoint = `/api/services/app/WorkflowStep/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteStepSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteStepError());
      });
  };

  const reorderAsync = async (input: IReorderWorkflowSteps) => {
    dispatch(reorderStepsPending());
    const endpoint = `/api/services/app/WorkflowStep/Reorder`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(reorderStepsSuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(reorderStepsError());
      });
  };

  return (
    <WorkflowStepStateContext.Provider value={state}>
      <WorkflowStepActionContext.Provider
        value={{
          getAsync,
          getByDefinitionAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          reorderAsync,
        }}
      >
        {children}
      </WorkflowStepActionContext.Provider>
    </WorkflowStepStateContext.Provider>
  );
};

export const useWorkflowStepState = () => {
  const context = useContext(WorkflowStepStateContext);
  if (!context) {
    throw new Error("useWorkflowStepState must be used within a WorkflowStepProvider");
  }
  return context;
};

export const useWorkflowStepActions = () => {
  const context = useContext(WorkflowStepActionContext);
  if (!context) {
    throw new Error("useWorkflowStepActions must be used within a WorkflowStepProvider");
  }
  return context;
};
