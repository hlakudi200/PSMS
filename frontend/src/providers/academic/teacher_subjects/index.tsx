"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  TeacherSubjectActionContext,
  TeacherSubjectStateContext,
} from "./context";
import {
  IAssignTeacherSubject,
} from "../shared/interfaces";
import { TeacherSubjectReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getByTeacherPending,
  getByTeacherSuccess,
  getByTeacherError,
  getBySubjectPending,
  getBySubjectSuccess,
  getBySubjectError,
  getByGradePending,
  getByGradeSuccess,
  getByGradeError,
  assignPending,
  assignSuccess,
  assignError,
  unassignPending,
  unassignSuccess,
  unassignError,
} from "./actions";

export const TeacherSubjectProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(TeacherSubjectReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getByTeacherAsync = async (teacherId: string) => {
    dispatch(getByTeacherPending());
    const endpoint = `/api/services/app/TeacherSubject/GetByTeacher?teacherId=${teacherId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByTeacherSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByTeacherError());
      });
  };

  const getBySubjectAsync = async (subjectId: string) => {
    dispatch(getBySubjectPending());
    const endpoint = `/api/services/app/TeacherSubject/GetBySubject?subjectId=${subjectId}`;
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

  const getByGradeAsync = async (gradeId: string) => {
    dispatch(getByGradePending());
    const endpoint = `/api/services/app/TeacherSubject/GetByGrade?gradeId=${gradeId}`;
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

  const assignAsync = async (input: IAssignTeacherSubject) => {
    dispatch(assignPending());
    const endpoint = `/api/services/app/TeacherSubject/Assign`;

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
    const endpoint = `/api/services/app/TeacherSubject/Unassign?id=${id}`;
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

  return (
    <TeacherSubjectStateContext.Provider value={state}>
      <TeacherSubjectActionContext.Provider
        value={{
          getByTeacherAsync,
          getBySubjectAsync,
          getByGradeAsync,
          assignAsync,
          unassignAsync,
        }}
      >
        {children}
      </TeacherSubjectActionContext.Provider>
    </TeacherSubjectStateContext.Provider>
  );
};

export const useTeacherSubjectState = () => {
  const context = useContext(TeacherSubjectStateContext);
  if (!context) {
    throw new Error("useTeacherSubjectState must be used within a TeacherSubjectProvider");
  }
  return context;
};

export const useTeacherSubjectActions = () => {
  const context = useContext(TeacherSubjectActionContext);
  if (!context) {
    throw new Error("useTeacherSubjectActions must be used within a TeacherSubjectProvider");
  }
  return context;
};
