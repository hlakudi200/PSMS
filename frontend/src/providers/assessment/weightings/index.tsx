"use client";
import { useContext, useReducer } from "react";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  AssessmentWeightingActionContext,
  AssessmentWeightingStateContext,
  IUpdateAssessmentWeightingItem,
} from "./context";
import { AssessmentWeightingReducer } from "./reducer";
import {
  getAllPending,
  getAllSuccess,
  getAllError,
  updatePending,
  updateSuccess,
  updateError,
  resetPending,
  resetSuccess,
  resetError,
} from "./actions";

const BASE = "/api/services/app/AssessmentWeighting";

export const AssessmentWeightingProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(AssessmentWeightingReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAllAsync = async () => {
    dispatch(getAllPending());
    await instance
      .get(BASE + "/GetAll")
      .then((response) => {
        dispatch(getAllSuccess(response.data.result.items));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllError());
        throw error;
      });
  };

  const updateAsync = async (weightings: IUpdateAssessmentWeightingItem[]) => {
    dispatch(updatePending());
    await instance
      .put(BASE + "/Update", { weightings })
      .then((response) => {
        dispatch(updateSuccess(response.data.result.items));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateError());
        throw error;
      });
  };

  const resetToDefaultsAsync = async () => {
    dispatch(resetPending());
    await instance
      .post(BASE + "/ResetToDefaults")
      .then((response) => {
        dispatch(resetSuccess(response.data.result.items));
      })
      .catch((error) => {
        console.error(error);
        dispatch(resetError());
        throw error;
      });
  };

  return (
    <AssessmentWeightingStateContext.Provider value={state}>
      <AssessmentWeightingActionContext.Provider
        value={{ getAllAsync, updateAsync, resetToDefaultsAsync }}
      >
        {children}
      </AssessmentWeightingActionContext.Provider>
    </AssessmentWeightingStateContext.Provider>
  );
};

export const useAssessmentWeightingState = () => {
  const context = useContext(AssessmentWeightingStateContext);
  if (!context) {
    throw new Error("useAssessmentWeightingState must be used within an AssessmentWeightingProvider");
  }
  return context;
};

export const useAssessmentWeightingActions = () => {
  const context = useContext(AssessmentWeightingActionContext);
  if (!context) {
    throw new Error("useAssessmentWeightingActions must be used within an AssessmentWeightingProvider");
  }
  return context;
};

export * from "./context";
