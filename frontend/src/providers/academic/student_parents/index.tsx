"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  StudentParentActionContext,
  StudentParentStateContext,
} from "./context";
import {
  ILinkStudentParent,
} from "../shared/interfaces";
import { StudentParentReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getByStudentPending,
  getByStudentSuccess,
  getByStudentError,
  getByParentPending,
  getByParentSuccess,
  getByParentError,
  linkPending,
  linkSuccess,
  linkError,
  updateLinkPending,
  updateLinkSuccess,
  updateLinkError,
  unlinkPending,
  unlinkSuccess,
  unlinkError,
} from "./actions";

export const StudentParentProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(StudentParentReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getByStudentAsync = async (studentId: string) => {
    dispatch(getByStudentPending());
    const endpoint = `/api/services/app/StudentParent/GetByStudent?studentId=${studentId}`;
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

  const getByParentAsync = async (parentId: string) => {
    dispatch(getByParentPending());
    const endpoint = `/api/services/app/StudentParent/GetByParent?parentId=${parentId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByParentSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByParentError());
      });
  };

  const linkAsync = async (input: ILinkStudentParent) => {
    dispatch(linkPending());
    const endpoint = `/api/services/app/StudentParent/Link`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(linkSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(linkError());
      });
  };

  const updateLinkAsync = async (id: string, input: ILinkStudentParent) => {
    dispatch(updateLinkPending());
    const endpoint = `/api/services/app/StudentParent/UpdateLink?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateLinkSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateLinkError());
      });
  };

  const unlinkAsync = async (id: string) => {
    dispatch(unlinkPending());
    const endpoint = `/api/services/app/StudentParent/Unlink?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(unlinkSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(unlinkError());
      });
  };

  return (
    <StudentParentStateContext.Provider value={state}>
      <StudentParentActionContext.Provider
        value={{
          getByStudentAsync,
          getByParentAsync,
          linkAsync,
          updateLinkAsync,
          unlinkAsync,
        }}
      >
        {children}
      </StudentParentActionContext.Provider>
    </StudentParentStateContext.Provider>
  );
};

export const useStudentParentState = () => {
  const context = useContext(StudentParentStateContext);
  if (!context) {
    throw new Error("useStudentParentState must be used within a StudentParentProvider");
  }
  return context;
};

export const useStudentParentActions = () => {
  const context = useContext(StudentParentActionContext);
  if (!context) {
    throw new Error("useStudentParentActions must be used within a StudentParentProvider");
  }
  return context;
};
