"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  AttendanceActionContext,
  AttendanceStateContext,
} from "./context";
import {
  ICaptureAttendance,
  IBulkCaptureAttendance,
  IUpdateAttendance,
  IGetAttendanceInput,
} from "../shared/interfaces";
import { AttendanceReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getAttendancesError,
  getAttendancesPending,
  getAttendancesSuccess,
  getAttendanceError,
  getAttendancePending,
  getAttendanceSuccess,
  getByStudentPending,
  getByStudentSuccess,
  getByStudentError,
  getByClassAndDatePending,
  getByClassAndDateSuccess,
  getByClassAndDateError,
  captureAttendancePending,
  captureAttendanceSuccess,
  captureAttendanceError,
  bulkCaptureAttendancePending,
  bulkCaptureAttendanceSuccess,
  bulkCaptureAttendanceError,
  updateAttendancePending,
  updateAttendanceSuccess,
  updateAttendanceError,
  deleteAttendancePending,
  deleteAttendanceSuccess,
  deleteAttendanceError,
  getStudentSummaryPending,
  getStudentSummarySuccess,
  getStudentSummaryError,
  getClassSummaryPending,
  getClassSummarySuccess,
  getClassSummaryError,
} from "./actions";

export const AttendanceProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(AttendanceReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getAttendancePending());
    const endpoint = `/api/services/app/Attendance/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAttendanceSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAttendanceError());
      });
  };

  const getAllAsync = async (input?: IGetAttendanceInput) => {
    dispatch(getAttendancesPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.studentId) params.append('StudentId', input.studentId);
    if (input?.classId) params.append('ClassId', input.classId);
    if (input?.startDate) params.append('StartDate', input.startDate);
    if (input?.endDate) params.append('EndDate', input.endDate);

    const endpoint = `/api/services/app/Attendance/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAttendancesSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAttendancesError());
      });
  };

  const getByStudentAsync = async (studentId: string, startDate?: string, endDate?: string) => {
    dispatch(getByStudentPending());

    const params = new URLSearchParams();
    params.append('studentId', studentId);
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);

    const endpoint = `/api/services/app/Attendance/GetByStudent?${params.toString()}`;
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

  const getByClassAndDateAsync = async (classId: string, date: string) => {
    dispatch(getByClassAndDatePending());

    const params = new URLSearchParams();
    params.append('classId', classId);
    params.append('date', date);

    const endpoint = `/api/services/app/Attendance/GetByClassAndDate?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByClassAndDateSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByClassAndDateError());
      });
  };

  const captureAsync = async (input: ICaptureAttendance) => {
    dispatch(captureAttendancePending());
    const endpoint = `/api/services/app/Attendance/Capture`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(captureAttendanceSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(captureAttendanceError());
      });
  };

  const bulkCaptureAsync = async (input: IBulkCaptureAttendance) => {
    dispatch(bulkCaptureAttendancePending());
    const endpoint = `/api/services/app/Attendance/BulkCapture`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(bulkCaptureAttendanceSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(bulkCaptureAttendanceError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateAttendance) => {
    dispatch(updateAttendancePending());
    const endpoint = `/api/services/app/Attendance/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateAttendanceSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateAttendanceError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteAttendancePending());
    const endpoint = `/api/services/app/Attendance/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteAttendanceSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteAttendanceError());
      });
  };

  const getStudentSummaryAsync = async (studentId: string, startDate: string, endDate: string) => {
    dispatch(getStudentSummaryPending());

    const params = new URLSearchParams();
    params.append('studentId', studentId);
    params.append('startDate', startDate);
    params.append('endDate', endDate);

    const endpoint = `/api/services/app/Attendance/GetStudentSummary?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getStudentSummarySuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getStudentSummaryError());
      });
  };

  const getClassSummaryAsync = async (classId: string, startDate: string, endDate: string) => {
    dispatch(getClassSummaryPending());

    const params = new URLSearchParams();
    params.append('classId', classId);
    params.append('startDate', startDate);
    params.append('endDate', endDate);

    const endpoint = `/api/services/app/Attendance/GetClassSummary?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getClassSummarySuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getClassSummaryError());
      });
  };

  return (
    <AttendanceStateContext.Provider value={state}>
      <AttendanceActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getByStudentAsync,
          getByClassAndDateAsync,
          captureAsync,
          bulkCaptureAsync,
          updateAsync,
          deleteAsync,
          getStudentSummaryAsync,
          getClassSummaryAsync,
        }}
      >
        {children}
      </AttendanceActionContext.Provider>
    </AttendanceStateContext.Provider>
  );
};

export const useAttendanceState = () => {
  const context = useContext(AttendanceStateContext);
  if (!context) {
    throw new Error("useAttendanceState must be used within an AttendanceProvider");
  }
  return context;
};

export const useAttendanceActions = () => {
  const context = useContext(AttendanceActionContext);
  if (!context) {
    throw new Error("useAttendanceActions must be used within an AttendanceProvider");
  }
  return context;
};
