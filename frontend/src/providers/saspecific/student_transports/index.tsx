"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  StudentTransportActionContext,
  StudentTransportStateContext,
} from "./context";
import {
  ICreateStudentTransport,
  IUpdateStudentTransport,
  IGetStudentTransportsInput,
} from "../shared/interfaces";
import { StudentTransportReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getStudentTransportPending,
  getStudentTransportSuccess,
  getStudentTransportError,
  getStudentTransportsPending,
  getStudentTransportsSuccess,
  getStudentTransportsError,
  getByTransportPending,
  getByTransportSuccess,
  getByTransportError,
  getByStudentPending,
  getByStudentSuccess,
  getByStudentError,
  createStudentTransportPending,
  createStudentTransportSuccess,
  createStudentTransportError,
  updateStudentTransportPending,
  updateStudentTransportSuccess,
  updateStudentTransportError,
  deleteStudentTransportPending,
  deleteStudentTransportSuccess,
  deleteStudentTransportError,
  suspendPending,
  suspendSuccess,
  suspendError,
  reactivatePending,
  reactivateSuccess,
  reactivateError,
  terminatePending,
  terminateSuccess,
  terminateError,
} from "./actions";

export const StudentTransportProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(StudentTransportReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getStudentTransportPending());
    const endpoint = `/api/services/app/StudentTransport/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getStudentTransportSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getStudentTransportError());
        throw error;
      });
  };

  const getAllAsync = async (input?: IGetStudentTransportsInput) => {
    dispatch(getStudentTransportsPending());
    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append("MaxResultCount", input.maxResultCount.toString());
    if (input?.skipCount) params.append("SkipCount", input.skipCount.toString());
    if (input?.sorting) params.append("Sorting", input.sorting);
    if (input?.studentId) params.append("StudentId", input.studentId);
    if (input?.schoolTransportId) params.append("SchoolTransportId", input.schoolTransportId);
    if (input?.academicYearId) params.append("AcademicYearId", input.academicYearId);
    if (input?.status !== undefined) params.append("Status", input.status.toString());
    if (input?.direction !== undefined) params.append("Direction", input.direction.toString());
    const endpoint = `/api/services/app/StudentTransport/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getStudentTransportsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getStudentTransportsError());
        throw error;
      });
  };

  const getByTransportAsync = async (transportId: string) => {
    dispatch(getByTransportPending());
    const endpoint = `/api/services/app/StudentTransport/GetByTransport?transportId=${transportId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByTransportSuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByTransportError());
        throw error;
      });
  };

  const getByStudentAsync = async (studentId: string) => {
    dispatch(getByStudentPending());
    const endpoint = `/api/services/app/StudentTransport/GetByStudent?studentId=${studentId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByStudentSuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByStudentError());
        throw error;
      });
  };

  const createAsync = async (input: ICreateStudentTransport) => {
    dispatch(createStudentTransportPending());
    const endpoint = `/api/services/app/StudentTransport/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createStudentTransportSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createStudentTransportError());
        throw error;
      });
  };

  const updateAsync = async (id: string, input: IUpdateStudentTransport) => {
    dispatch(updateStudentTransportPending());
    const endpoint = `/api/services/app/StudentTransport/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateStudentTransportSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateStudentTransportError());
        throw error;
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteStudentTransportPending());
    const endpoint = `/api/services/app/StudentTransport/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteStudentTransportSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteStudentTransportError());
        throw error;
      });
  };

  const suspendAsync = async (id: string) => {
    dispatch(suspendPending());
    const endpoint = `/api/services/app/StudentTransport/Suspend?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(suspendSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(suspendError());
        throw error;
      });
  };

  const reactivateAsync = async (id: string) => {
    dispatch(reactivatePending());
    const endpoint = `/api/services/app/StudentTransport/Reactivate?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(reactivateSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(reactivateError());
        throw error;
      });
  };

  const terminateAsync = async (id: string) => {
    dispatch(terminatePending());
    const endpoint = `/api/services/app/StudentTransport/Terminate?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(terminateSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(terminateError());
        throw error;
      });
  };

  return (
    <StudentTransportStateContext.Provider value={state}>
      <StudentTransportActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getByTransportAsync,
          getByStudentAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          suspendAsync,
          reactivateAsync,
          terminateAsync,
        }}
      >
        {children}
      </StudentTransportActionContext.Provider>
    </StudentTransportStateContext.Provider>
  );
};

export const useStudentTransportState = () => {
  const context = useContext(StudentTransportStateContext);
  if (!context) {
    throw new Error("useStudentTransportState must be used within a StudentTransportProvider");
  }
  return context;
};

export const useStudentTransportActions = () => {
  const context = useContext(StudentTransportActionContext);
  if (!context) {
    throw new Error("useStudentTransportActions must be used within a StudentTransportProvider");
  }
  return context;
};
