"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import { buildQueryParams } from "@/utils/query-params";
import {
  INITIAL_STATE,
  StudentActionContext,
  StudentStateContext,
} from "./context";
import {
  ICreateStudent,
  IUpdateStudent,
  IGetStudentsInput,
} from "../shared/interfaces";
import { StudentReducer } from "./reducer";
import { useContext, useReducer, useCallback, useMemo } from "react";
import {
  getStudentsError,
  getStudentsPending,
  getStudentsSuccess,
  getStudentError,
  getStudentPending,
  getStudentSuccess,
  createStudentPending,
  createStudentError,
  updateStudentSuccess,
  createStudentSuccess,
  updateStudentPending,
  updateStudentError,
  deleteStudentPending,
  deleteStudentSuccess,
  deleteStudentError,
} from "./actions";

export const StudentProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(StudentReducer, INITIAL_STATE);
  const instance = useMemo(() => getAxiosInstance(), []);

  const getAsync = useCallback(async (id: string) => {
    dispatch(getStudentPending());
    const endpoint = `/api/services/app/Student/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getStudentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getStudentError());
      });
  }, [instance]);

  const getAllAsync = useCallback(async (input?: IGetStudentsInput) => {
    dispatch(getStudentsPending());

    const params = buildQueryParams(input as Record<string, unknown>);
    const endpoint = `/api/services/app/Student/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getStudentsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getStudentsError());
      });
  }, [instance]);

  const createAsync = useCallback(async (input: ICreateStudent) => {
    dispatch(createStudentPending());
    const endpoint = `/api/services/app/Student/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createStudentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createStudentError());
      });
  }, [instance]);

  const updateAsync = useCallback(async (id: string, input: IUpdateStudent) => {
    dispatch(updateStudentPending());
    const endpoint = `/api/services/app/Student/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateStudentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateStudentError());
      });
  }, [instance]);

  const deleteAsync = useCallback(async (id: string) => {
    dispatch(deleteStudentPending());
    const endpoint = `/api/services/app/Student/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteStudentSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteStudentError());
      });
  }, [instance]);

  const actionValue = useMemo(() => ({
    getAsync,
    getAllAsync,
    createAsync,
    updateAsync,
    deleteAsync,
  }), [getAsync, getAllAsync, createAsync, updateAsync, deleteAsync]);

  return (
    <StudentStateContext.Provider value={state}>
      <StudentActionContext.Provider value={actionValue}>
        {children}
      </StudentActionContext.Provider>
    </StudentStateContext.Provider>
  );
};

export const useStudentState = () => {
  const context = useContext(StudentStateContext);
  if (!context) {
    throw new Error("useStudentState must be used within a StudentProvider");
  }
  return context;
};

export const useStudentActions = () => {
  const context = useContext(StudentActionContext);
  if (!context) {
    throw new Error("useStudentActions must be used within a StudentProvider");
  }
  return context;
};
