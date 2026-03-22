"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  StudentClassActionContext,
  StudentClassStateContext,
} from "./context";
import {
  ICreateStudentClass,
  IUpdateStudentClass,
} from "../shared/interfaces";
import { StudentClassReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getStudentClassPending,
  getStudentClassSuccess,
  getStudentClassError,
  getByStudentPending,
  getByStudentSuccess,
  getByStudentError,
  getByClassPending,
  getByClassSuccess,
  getByClassError,
  getCurrentByStudentPending,
  getCurrentByStudentSuccess,
  getCurrentByStudentError,
  enrollPending,
  enrollSuccess,
  enrollError,
  updatePending,
  updateSuccess,
  updateError,
  endEnrollmentPending,
  endEnrollmentSuccess,
  endEnrollmentError,
  setAsCurrentPending,
  setAsCurrentSuccess,
  setAsCurrentError,
  deleteStudentClassPending,
  deleteStudentClassSuccess,
  deleteStudentClassError,
} from "./actions";

export const StudentClassProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(StudentClassReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getStudentClassPending());
    const endpoint = `/api/services/app/StudentClass/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getStudentClassSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getStudentClassError());
      });
  };

  const getByStudentAsync = async (studentId: string) => {
    dispatch(getByStudentPending());
    const endpoint = `/api/services/app/StudentClass/GetByStudent?studentId=${studentId}`;
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

  const getByClassAsync = async (classId: string) => {
    dispatch(getByClassPending());
    const endpoint = `/api/services/app/StudentClass/GetByClass?classId=${classId}`;
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

  const getCurrentByStudentAsync = async (studentId: string) => {
    dispatch(getCurrentByStudentPending());
    const endpoint = `/api/services/app/StudentClass/GetCurrentByStudent?studentId=${studentId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getCurrentByStudentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getCurrentByStudentError());
      });
  };

  const enrollAsync = async (input: ICreateStudentClass) => {
    dispatch(enrollPending());
    const endpoint = `/api/services/app/StudentClass/Enroll`;

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

  const updateAsync = async (id: string, input: IUpdateStudentClass) => {
    dispatch(updatePending());
    const endpoint = `/api/services/app/StudentClass/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateError());
      });
  };

  const endEnrollmentAsync = async (id: string, endDate: string) => {
    dispatch(endEnrollmentPending());
    const endpoint = `/api/services/app/StudentClass/EndEnrollment?id=${id}`;
    await instance
      .put(endpoint, { endDate })
      .then((response) => {
        dispatch(endEnrollmentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(endEnrollmentError());
      });
  };

  const setAsCurrentAsync = async (id: string) => {
    dispatch(setAsCurrentPending());
    const endpoint = `/api/services/app/StudentClass/SetAsCurrent?id=${id}`;
    await instance
      .put(endpoint)
      .then((response) => {
        dispatch(setAsCurrentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(setAsCurrentError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteStudentClassPending());
    const endpoint = `/api/services/app/StudentClass/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteStudentClassSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteStudentClassError());
      });
  };

  return (
    <StudentClassStateContext.Provider value={state}>
      <StudentClassActionContext.Provider
        value={{
          getAsync,
          getByStudentAsync,
          getByClassAsync,
          getCurrentByStudentAsync,
          enrollAsync,
          updateAsync,
          endEnrollmentAsync,
          setAsCurrentAsync,
          deleteAsync,
        }}
      >
        {children}
      </StudentClassActionContext.Provider>
    </StudentClassStateContext.Provider>
  );
};

export const useStudentClassState = () => {
  const context = useContext(StudentClassStateContext);
  if (!context) {
    throw new Error("useStudentClassState must be used within a StudentClassProvider");
  }
  return context;
};

export const useStudentClassActions = () => {
  const context = useContext(StudentClassActionContext);
  if (!context) {
    throw new Error("useStudentClassActions must be used within a StudentClassProvider");
  }
  return context;
};
