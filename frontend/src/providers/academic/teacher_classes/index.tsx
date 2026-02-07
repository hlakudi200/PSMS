"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  TeacherClassActionContext,
  TeacherClassStateContext,
} from "./context";
import {
  IAssignTeacherClass,
} from "../shared/interfaces";
import { TeacherClassReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getByTeacherPending,
  getByTeacherSuccess,
  getByTeacherError,
  getByClassPending,
  getByClassSuccess,
  getByClassError,
  assignPending,
  assignSuccess,
  assignError,
  unassignPending,
  unassignSuccess,
  unassignError,
} from "./actions";

export const TeacherClassProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(TeacherClassReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getByTeacherAsync = async (teacherId: string) => {
    dispatch(getByTeacherPending());
    const endpoint = `/api/services/app/TeacherClass/GetByTeacher?teacherId=${teacherId}`;
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

  const getByClassAsync = async (classId: string) => {
    dispatch(getByClassPending());
    const endpoint = `/api/services/app/TeacherClass/GetByClass?classId=${classId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByClassSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByClassError());
      });
  };

  const assignAsync = async (input: IAssignTeacherClass) => {
    dispatch(assignPending());
    const endpoint = `/api/services/app/TeacherClass/Assign`;

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
    const endpoint = `/api/services/app/TeacherClass/Unassign?id=${id}`;
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
    <TeacherClassStateContext.Provider value={state}>
      <TeacherClassActionContext.Provider
        value={{
          getByTeacherAsync,
          getByClassAsync,
          assignAsync,
          unassignAsync,
        }}
      >
        {children}
      </TeacherClassActionContext.Provider>
    </TeacherClassStateContext.Provider>
  );
};

export const useTeacherClassState = () => {
  const context = useContext(TeacherClassStateContext);
  if (!context) {
    throw new Error("useTeacherClassState must be used within a TeacherClassProvider");
  }
  return context;
};

export const useTeacherClassActions = () => {
  const context = useContext(TeacherClassActionContext);
  if (!context) {
    throw new Error("useTeacherClassActions must be used within a TeacherClassProvider");
  }
  return context;
};
