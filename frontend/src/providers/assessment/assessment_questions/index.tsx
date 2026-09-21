"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  AssessmentQuestionActionContext,
  AssessmentQuestionStateContext,
} from "./context";
import {
  ICreateAssessmentQuestion,
  IUpdateAssessmentQuestion,
  IReorderQuestions,
} from "../shared/interfaces";
import { AssessmentQuestionReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getQuestionPending,
  getQuestionSuccess,
  getQuestionError,
  getByAssessmentPending,
  getByAssessmentSuccess,
  getByAssessmentError,
  createQuestionPending,
  createQuestionSuccess,
  createQuestionError,
  updateQuestionPending,
  updateQuestionSuccess,
  updateQuestionError,
  deleteQuestionPending,
  deleteQuestionSuccess,
  deleteQuestionError,
  reorderPending,
  reorderSuccess,
  reorderError,
} from "./actions";

export const AssessmentQuestionProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(AssessmentQuestionReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getQuestionPending());
    const endpoint = `/api/services/app/AssessmentQuestion/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getQuestionSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getQuestionError());
        throw error;
      });
  };

  const getByAssessmentAsync = async (assessmentId: string) => {
    dispatch(getByAssessmentPending());
    const endpoint = `/api/services/app/AssessmentQuestion/GetByAssessment?assessmentId=${assessmentId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByAssessmentSuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByAssessmentError());
        throw error;
      });
  };

  const createAsync = async (input: ICreateAssessmentQuestion) => {
    dispatch(createQuestionPending());
    const endpoint = `/api/services/app/AssessmentQuestion/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createQuestionSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createQuestionError());
        throw error;
      });
  };

  const updateAsync = async (id: string, input: IUpdateAssessmentQuestion) => {
    dispatch(updateQuestionPending());
    const endpoint = `/api/services/app/AssessmentQuestion/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateQuestionSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateQuestionError());
        throw error;
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteQuestionPending());
    const endpoint = `/api/services/app/AssessmentQuestion/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteQuestionSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteQuestionError());
        throw error;
      });
  };

  const reorderAsync = async (input: IReorderQuestions) => {
    dispatch(reorderPending());
    const endpoint = `/api/services/app/AssessmentQuestion/Reorder`;
    await instance
      .post(endpoint, input)
      .then(() => {
        dispatch(reorderSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(reorderError());
        throw error;
      });
  };

  return (
    <AssessmentQuestionStateContext.Provider value={state}>
      <AssessmentQuestionActionContext.Provider
        value={{
          getAsync,
          getByAssessmentAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          reorderAsync,
        }}
      >
        {children}
      </AssessmentQuestionActionContext.Provider>
    </AssessmentQuestionStateContext.Provider>
  );
};

export const useAssessmentQuestionState = () => {
  const context = useContext(AssessmentQuestionStateContext);
  if (!context) {
    throw new Error("useAssessmentQuestionState must be used within an AssessmentQuestionProvider");
  }
  return context;
};

export const useAssessmentQuestionActions = () => {
  const context = useContext(AssessmentQuestionActionContext);
  if (!context) {
    throw new Error("useAssessmentQuestionActions must be used within an AssessmentQuestionProvider");
  }
  return context;
};
