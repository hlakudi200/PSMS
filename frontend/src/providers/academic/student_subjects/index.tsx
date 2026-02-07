"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  StudentSubjectActionContext,
  StudentSubjectStateContext,
} from "./context";
import {
  IEnrollStudentSubject,
} from "../shared/interfaces";
import { StudentSubjectReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getByStudentPending,
  getByStudentSuccess,
  getByStudentError,
  getBySubjectPending,
  getBySubjectSuccess,
  getBySubjectError,
  getByStudentAndYearPending,
  getByStudentAndYearSuccess,
  getByStudentAndYearError,
  enrollPending,
  enrollSuccess,
  enrollError,
  unenrollPending,
  unenrollSuccess,
  unenrollError,
} from "./actions";

export const StudentSubjectProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(StudentSubjectReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getByStudentAsync = async (studentId: string) => {
    dispatch(getByStudentPending());
    const endpoint = `/api/services/app/StudentSubject/GetByStudent?studentId=${studentId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByStudentSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByStudentError());
      });
  };

  const getBySubjectAsync = async (subjectId: string) => {
    dispatch(getBySubjectPending());
    const endpoint = `/api/services/app/StudentSubject/GetBySubject?subjectId=${subjectId}`;
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

  const getByStudentAndYearAsync = async (studentId: string, academicYearId: string) => {
    dispatch(getByStudentAndYearPending());
    const endpoint = `/api/services/app/StudentSubject/GetByStudentAndYear?studentId=${studentId}&academicYearId=${academicYearId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByStudentAndYearSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByStudentAndYearError());
      });
  };

  const enrollAsync = async (input: IEnrollStudentSubject) => {
    dispatch(enrollPending());
    const endpoint = `/api/services/app/StudentSubject/Enroll`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(enrollSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(enrollError());
      });
  };

  const unenrollAsync = async (id: string) => {
    dispatch(unenrollPending());
    const endpoint = `/api/services/app/StudentSubject/Unenroll?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(unenrollSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(unenrollError());
      });
  };

  return (
    <StudentSubjectStateContext.Provider value={state}>
      <StudentSubjectActionContext.Provider
        value={{
          getByStudentAsync,
          getBySubjectAsync,
          getByStudentAndYearAsync,
          enrollAsync,
          unenrollAsync,
        }}
      >
        {children}
      </StudentSubjectActionContext.Provider>
    </StudentSubjectStateContext.Provider>
  );
};

export const useStudentSubjectState = () => {
  const context = useContext(StudentSubjectStateContext);
  if (!context) {
    throw new Error("useStudentSubjectState must be used within a StudentSubjectProvider");
  }
  return context;
};

export const useStudentSubjectActions = () => {
  const context = useContext(StudentSubjectActionContext);
  if (!context) {
    throw new Error("useStudentSubjectActions must be used within a StudentSubjectProvider");
  }
  return context;
};
