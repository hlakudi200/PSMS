"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import { buildQueryParams } from "@/utils/query-params";
import {
  INITIAL_STATE,
  TimetableActionContext,
  TimetableStateContext,
} from "./context";
import {
  ICreateTimetable,
  IUpdateTimetable,
  IPagedAndSortedResultRequest,
  IGenerateTimetablesInput,
} from "../shared/interfaces";
import { TimetableReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getTimetablesError,
  getTimetablesPending,
  getTimetablesSuccess,
  getTimetableError,
  getTimetablePending,
  getTimetableSuccess,
  getByClassPending,
  getByClassSuccess,
  getByClassError,
  createTimetablePending,
  createTimetableError,
  updateTimetableSuccess,
  createTimetableSuccess,
  updateTimetablePending,
  updateTimetableError,
  deleteTimetablePending,
  deleteTimetableSuccess,
  deleteTimetableError,
  activateTimetablePending,
  activateTimetableSuccess,
  activateTimetableError,
  deactivateTimetablePending,
  deactivateTimetableSuccess,
  deactivateTimetableError,
  generateTimetablesPending,
  generateTimetablesSuccess,
  generateTimetablesError,
} from "./actions";

export const TimetableProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(TimetableReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getTimetablePending());
    const endpoint = `/api/services/app/Timetable/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getTimetableSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getTimetableError());
        throw error;
      });
  };

  const getAllAsync = async (input?: IPagedAndSortedResultRequest) => {
    dispatch(getTimetablesPending());

    const params = buildQueryParams(input as Record<string, unknown>);
    const endpoint = `/api/services/app/Timetable/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getTimetablesSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getTimetablesError());
        throw error;
      });
  };

  const getByClassAsync = async (classId: string) => {
    dispatch(getByClassPending());
    const endpoint = `/api/services/app/Timetable/GetByClass?classId=${classId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByClassSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByClassError());
        throw error;
      });
  };

  const createAsync = async (input: ICreateTimetable) => {
    dispatch(createTimetablePending());
    const endpoint = `/api/services/app/Timetable/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createTimetableSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createTimetableError());
        throw error;
      });
  };

  const updateAsync = async (id: string, input: IUpdateTimetable) => {
    dispatch(updateTimetablePending());
    const endpoint = `/api/services/app/Timetable/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateTimetableSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateTimetableError());
        throw error;
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteTimetablePending());
    const endpoint = `/api/services/app/Timetable/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteTimetableSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteTimetableError());
        throw error;
      });
  };

  const activateAsync = async (id: string) => {
    dispatch(activateTimetablePending());
    const endpoint = `/api/services/app/Timetable/Activate?id=${id}`;
    await instance
      .put(endpoint)
      .then((response) => {
        dispatch(activateTimetableSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(activateTimetableError());
        throw error;
      });
  };

  const deactivateAsync = async (id: string) => {
    dispatch(deactivateTimetablePending());
    const endpoint = `/api/services/app/Timetable/Deactivate?id=${id}`;
    await instance
      .put(endpoint)
      .then((response) => {
        dispatch(deactivateTimetableSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(deactivateTimetableError());
        throw error;
      });
  };

  const generateAsync = async (input: IGenerateTimetablesInput) => {
    dispatch(generateTimetablesPending());
    const endpoint = `/api/services/app/Timetable/Generate`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(generateTimetablesSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(generateTimetablesError());
        throw error;
      });
  };

  return (
    <TimetableStateContext.Provider value={state}>
      <TimetableActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getByClassAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          activateAsync,
          deactivateAsync,
          generateAsync,
        }}
      >
        {children}
      </TimetableActionContext.Provider>
    </TimetableStateContext.Provider>
  );
};

export const useTimetableState = () => {
  const context = useContext(TimetableStateContext);
  if (!context) {
    throw new Error("useTimetableState must be used within a TimetableProvider");
  }
  return context;
};

export const useTimetableActions = () => {
  const context = useContext(TimetableActionContext);
  if (!context) {
    throw new Error("useTimetableActions must be used within a TimetableProvider");
  }
  return context;
};
