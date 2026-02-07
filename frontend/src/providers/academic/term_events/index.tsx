"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  TermEventActionContext,
  TermEventStateContext,
} from "./context";
import {
  ICreateTermEvent,
  IUpdateTermEvent,
} from "../shared/interfaces";
import { TermEventReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getTermEventPending,
  getTermEventSuccess,
  getTermEventError,
  getByTermPending,
  getByTermSuccess,
  getByTermError,
  getByDateRangePending,
  getByDateRangeSuccess,
  getByDateRangeError,
  createTermEventPending,
  createTermEventSuccess,
  createTermEventError,
  updateTermEventPending,
  updateTermEventSuccess,
  updateTermEventError,
  deleteTermEventPending,
  deleteTermEventSuccess,
  deleteTermEventError,
} from "./actions";

export const TermEventProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(TermEventReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getTermEventPending());
    const endpoint = `/api/services/app/TermEvent/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getTermEventSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getTermEventError());
      });
  };

  const getByTermAsync = async (termId: string) => {
    dispatch(getByTermPending());
    const endpoint = `/api/services/app/TermEvent/GetByTerm?termId=${termId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByTermSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByTermError());
      });
  };

  const getByDateRangeAsync = async (startDate: string, endDate: string) => {
    dispatch(getByDateRangePending());
    const endpoint = `/api/services/app/TermEvent/GetByDateRange?startDate=${startDate}&endDate=${endDate}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByDateRangeSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByDateRangeError());
      });
  };

  const createAsync = async (input: ICreateTermEvent) => {
    dispatch(createTermEventPending());
    const endpoint = `/api/services/app/TermEvent/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createTermEventSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createTermEventError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateTermEvent) => {
    dispatch(updateTermEventPending());
    const endpoint = `/api/services/app/TermEvent/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(updateTermEventSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateTermEventError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteTermEventPending());
    const endpoint = `/api/services/app/TermEvent/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteTermEventSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteTermEventError());
      });
  };

  return (
    <TermEventStateContext.Provider value={state}>
      <TermEventActionContext.Provider
        value={{
          getAsync,
          getByTermAsync,
          getByDateRangeAsync,
          createAsync,
          updateAsync,
          deleteAsync,
        }}
      >
        {children}
      </TermEventActionContext.Provider>
    </TermEventStateContext.Provider>
  );
};

export const useTermEventState = () => {
  const context = useContext(TermEventStateContext);
  if (!context) {
    throw new Error("useTermEventState must be used within a TermEventProvider");
  }
  return context;
};

export const useTermEventActions = () => {
  const context = useContext(TermEventActionContext);
  if (!context) {
    throw new Error("useTermEventActions must be used within a TermEventProvider");
  }
  return context;
};
