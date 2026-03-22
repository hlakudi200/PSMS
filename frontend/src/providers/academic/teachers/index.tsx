"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import { buildQueryParams } from "@/utils/query-params";
import {
  INITIAL_STATE,
  TeacherActionContext,
  TeacherStateContext,
} from "./context";
import {
  ITeacher,
  ICreateTeacher,
  IUpdateTeacher,
  IPagedAndSortedResultRequest,
} from "../shared/interfaces";
import { TeacherReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getTeachersError,
  getTeachersPending,
  getTeachersSuccess,
  getTeacherError,
  getTeacherPending,
  getTeacherSuccess,
  createTeacherPending,
  createTeacherError,
  updateTeacherSuccess,
  createTeacherSuccess,
  updateTeacherPending,
  updateTeacherError,
  deleteTeacherPending,
  deleteTeacherSuccess,
  deleteTeacherError,
  activateTeacherPending,
  activateTeacherSuccess,
  activateTeacherError,
  deactivateTeacherPending,
  deactivateTeacherSuccess,
  deactivateTeacherError,
} from "./actions";

export const TeacherProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(TeacherReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getTeacherPending());
    const endpoint = `/api/services/app/Teacher/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getTeacherSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getTeacherError());
      });
  };

  const getAllAsync = async (input?: IPagedAndSortedResultRequest) => {
    dispatch(getTeachersPending());

    const params = buildQueryParams(input as Record<string, unknown>);
    const endpoint = `/api/services/app/Teacher/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getTeachersSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getTeachersError());
      });
  };

  const createAsync = async (input: ICreateTeacher) => {
    dispatch(createTeacherPending());
    const endpoint = `/api/services/app/Teacher/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createTeacherSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createTeacherError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateTeacher) => {
    dispatch(updateTeacherPending());
    const endpoint = `/api/services/app/Teacher/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateTeacherSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateTeacherError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteTeacherPending());
    const endpoint = `/api/services/app/Teacher/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteTeacherSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteTeacherError());
      });
  };

  const activateAsync = async (id: string) => {
    dispatch(activateTeacherPending());
    const endpoint = `/api/services/app/Teacher/Update?id=${id}`;
    await instance
      .put(endpoint, { isActive: true })
      .then(() => {
        dispatch(activateTeacherSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(activateTeacherError());
      });
  };

  const deactivateAsync = async (id: string) => {
    dispatch(deactivateTeacherPending());
    const endpoint = `/api/services/app/Teacher/Update?id=${id}`;
    await instance
      .put(endpoint, { isActive: false })
      .then(() => {
        dispatch(deactivateTeacherSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deactivateTeacherError());
      });
  };

  return (
    <TeacherStateContext.Provider value={state}>
      <TeacherActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          activateAsync,
          deactivateAsync,
        }}
      >
        {children}
      </TeacherActionContext.Provider>
    </TeacherStateContext.Provider>
  );
};

export const useTeacherState = () => {
  const context = useContext(TeacherStateContext);
  if (!context) {
    throw new Error("useTeacherState must be used within a TeacherProvider");
  }
  return context;
};

export const useTeacherActions = () => {
  const context = useContext(TeacherActionContext);
  if (!context) {
    throw new Error("useTeacherActions must be used within a TeacherProvider");
  }
  return context;
};
