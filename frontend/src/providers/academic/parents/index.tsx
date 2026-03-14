"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import { buildQueryParams } from "@/utils/query-params";
import {
  INITIAL_STATE,
  ParentActionContext,
  ParentStateContext,
} from "./context";
import {
  IParent,
  ICreateParent,
  IUpdateParent,
  IPagedAndSortedResultRequest,
} from "../shared/interfaces";
import { ParentReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getParentsError,
  getParentsPending,
  getParentsSuccess,
  getParentError,
  getParentPending,
  getParentSuccess,
  createParentPending,
  createParentError,
  updateParentSuccess,
  createParentSuccess,
  updateParentPending,
  updateParentError,
  deleteParentPending,
  deleteParentSuccess,
  deleteParentError,
} from "./actions";

export const ParentProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(ParentReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getParentPending());
    const endpoint = `/api/services/app/Parent/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getParentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getParentError());
      });
  };

  const getAllAsync = async (input?: IPagedAndSortedResultRequest) => {
    dispatch(getParentsPending());

    const params = buildQueryParams(input as Record<string, unknown>);
    const endpoint = `/api/services/app/Parent/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getParentsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getParentsError());
      });
  };

  const createAsync = async (input: ICreateParent) => {
    dispatch(createParentPending());
    const endpoint = `/api/services/app/Parent/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createParentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createParentError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateParent) => {
    dispatch(updateParentPending());
    const endpoint = `/api/services/app/Parent/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateParentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateParentError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteParentPending());
    const endpoint = `/api/services/app/Parent/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteParentSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteParentError());
      });
  };

  return (
    <ParentStateContext.Provider value={state}>
      <ParentActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          createAsync,
          updateAsync,
          deleteAsync,
        }}
      >
        {children}
      </ParentActionContext.Provider>
    </ParentStateContext.Provider>
  );
};

export const useParentState = () => {
  const context = useContext(ParentStateContext);
  if (!context) {
    throw new Error("useParentState must be used within a ParentProvider");
  }
  return context;
};

export const useParentActions = () => {
  const context = useContext(ParentActionContext);
  if (!context) {
    throw new Error("useParentActions must be used within a ParentProvider");
  }
  return context;
};
