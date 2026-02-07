"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  TimetableSlotActionContext,
  TimetableSlotStateContext,
} from "./context";
import {
  ICreateTimetableSlot,
  IUpdateTimetableSlot,
} from "../shared/interfaces";
import { TimetableSlotReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getTimetableSlotPending,
  getTimetableSlotSuccess,
  getTimetableSlotError,
  getByTimetablePending,
  getByTimetableSuccess,
  getByTimetableError,
  getByDayPending,
  getByDaySuccess,
  getByDayError,
  getByTeacherPending,
  getByTeacherSuccess,
  getByTeacherError,
  createTimetableSlotPending,
  createTimetableSlotSuccess,
  createTimetableSlotError,
  updateTimetableSlotPending,
  updateTimetableSlotSuccess,
  updateTimetableSlotError,
  deleteTimetableSlotPending,
  deleteTimetableSlotSuccess,
  deleteTimetableSlotError,
  bulkCreateTimetableSlotPending,
  bulkCreateTimetableSlotSuccess,
  bulkCreateTimetableSlotError,
} from "./actions";

export const TimetableSlotProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(TimetableSlotReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getTimetableSlotPending());
    const endpoint = `/api/services/app/TimetableSlot/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getTimetableSlotSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getTimetableSlotError());
      });
  };

  const getByTimetableAsync = async (timetableId: string) => {
    dispatch(getByTimetablePending());
    const endpoint = `/api/services/app/TimetableSlot/GetByTimetable?timetableId=${timetableId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByTimetableSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByTimetableError());
      });
  };

  const getByDayAsync = async (timetableId: string, day: number) => {
    dispatch(getByDayPending());
    const endpoint = `/api/services/app/TimetableSlot/GetByDay?timetableId=${timetableId}&day=${day}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByDaySuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByDayError());
      });
  };

  const getByTeacherAsync = async (teacherId: string) => {
    dispatch(getByTeacherPending());
    const endpoint = `/api/services/app/TimetableSlot/GetByTeacher?teacherId=${teacherId}`;
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

  const createAsync = async (input: ICreateTimetableSlot) => {
    dispatch(createTimetableSlotPending());
    const endpoint = `/api/services/app/TimetableSlot/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createTimetableSlotSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createTimetableSlotError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateTimetableSlot) => {
    dispatch(updateTimetableSlotPending());
    const endpoint = `/api/services/app/TimetableSlot/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(updateTimetableSlotSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateTimetableSlotError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteTimetableSlotPending());
    const endpoint = `/api/services/app/TimetableSlot/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteTimetableSlotSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteTimetableSlotError());
      });
  };

  const bulkCreateAsync = async (timetableId: string, input: ICreateTimetableSlot[]) => {
    dispatch(bulkCreateTimetableSlotPending());
    const endpoint = `/api/services/app/TimetableSlot/BulkCreate?timetableId=${timetableId}`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(bulkCreateTimetableSlotSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(bulkCreateTimetableSlotError());
      });
  };

  return (
    <TimetableSlotStateContext.Provider value={state}>
      <TimetableSlotActionContext.Provider
        value={{
          getAsync,
          getByTimetableAsync,
          getByDayAsync,
          getByTeacherAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          bulkCreateAsync,
        }}
      >
        {children}
      </TimetableSlotActionContext.Provider>
    </TimetableSlotStateContext.Provider>
  );
};

export const useTimetableSlotState = () => {
  const context = useContext(TimetableSlotStateContext);
  if (!context) {
    throw new Error("useTimetableSlotState must be used within a TimetableSlotProvider");
  }
  return context;
};

export const useTimetableSlotActions = () => {
  const context = useContext(TimetableSlotActionContext);
  if (!context) {
    throw new Error("useTimetableSlotActions must be used within a TimetableSlotProvider");
  }
  return context;
};
