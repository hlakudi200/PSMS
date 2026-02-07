"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  GradeSubjectActionContext,
  GradeSubjectStateContext,
} from "./context";
import {
  IAssignSubjectToGrade,
} from "../shared/interfaces";
import { GradeSubjectReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getByGradePending,
  getByGradeSuccess,
  getByGradeError,
  getBySubjectPending,
  getBySubjectSuccess,
  getBySubjectError,
  assignPending,
  assignSuccess,
  assignError,
  unassignPending,
  unassignSuccess,
  unassignError,
  getUnassignedSubjectsPending,
  getUnassignedSubjectsSuccess,
  getUnassignedSubjectsError,
} from "./actions";

export const GradeSubjectProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(GradeSubjectReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getByGradeAsync = async (gradeId: string) => {
    dispatch(getByGradePending());
    const endpoint = `/api/services/app/GradeSubject/GetByGrade?gradeId=${gradeId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByGradeSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByGradeError());
      });
  };

  const getBySubjectAsync = async (subjectId: string) => {
    dispatch(getBySubjectPending());
    const endpoint = `/api/services/app/GradeSubject/GetBySubject?subjectId=${subjectId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getBySubjectSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getBySubjectError());
      });
  };

  const assignAsync = async (input: IAssignSubjectToGrade) => {
    dispatch(assignPending());
    const endpoint = `/api/services/app/GradeSubject/Assign`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(assignSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(assignError());
      });
  };

  const unassignAsync = async (id: string) => {
    dispatch(unassignPending());
    const endpoint = `/api/services/app/GradeSubject/Unassign?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(unassignSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(unassignError());
      });
  };

  const getUnassignedSubjectsForGradeAsync = async (gradeId: string) => {
    dispatch(getUnassignedSubjectsPending());
    const endpoint = `/api/services/app/GradeSubject/GetUnassignedSubjectsForGrade?gradeId=${gradeId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getUnassignedSubjectsSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getUnassignedSubjectsError());
      });
  };

  return (
    <GradeSubjectStateContext.Provider value={state}>
      <GradeSubjectActionContext.Provider
        value={{
          getByGradeAsync,
          getBySubjectAsync,
          assignAsync,
          unassignAsync,
          getUnassignedSubjectsForGradeAsync,
        }}
      >
        {children}
      </GradeSubjectActionContext.Provider>
    </GradeSubjectStateContext.Provider>
  );
};

export const useGradeSubjectState = () => {
  const context = useContext(GradeSubjectStateContext);
  if (!context) {
    throw new Error("useGradeSubjectState must be used within a GradeSubjectProvider");
  }
  return context;
};

export const useGradeSubjectActions = () => {
  const context = useContext(GradeSubjectActionContext);
  if (!context) {
    throw new Error("useGradeSubjectActions must be used within a GradeSubjectProvider");
  }
  return context;
};
