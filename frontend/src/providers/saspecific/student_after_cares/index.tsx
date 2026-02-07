"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  StudentAfterCareActionContext,
  StudentAfterCareStateContext,
} from "./context";
import {
  ICreateStudentAfterCare,
  IUpdateStudentAfterCare,
  IGetStudentAfterCaresInput,
} from "../shared/interfaces";
import { StudentAfterCareReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getStudentAfterCarePending,
  getStudentAfterCareSuccess,
  getStudentAfterCareError,
  getAllStudentAfterCaresPending,
  getAllStudentAfterCaresSuccess,
  getAllStudentAfterCaresError,
  getByAfterCarePending,
  getByAfterCareSuccess,
  getByAfterCareError,
  getByStudentPending,
  getByStudentSuccess,
  getByStudentError,
  createStudentAfterCarePending,
  createStudentAfterCareSuccess,
  createStudentAfterCareError,
  updateStudentAfterCarePending,
  updateStudentAfterCareSuccess,
  updateStudentAfterCareError,
  deleteStudentAfterCarePending,
  deleteStudentAfterCareSuccess,
  deleteStudentAfterCareError,
  suspendStudentAfterCarePending,
  suspendStudentAfterCareSuccess,
  suspendStudentAfterCareError,
  reactivateStudentAfterCarePending,
  reactivateStudentAfterCareSuccess,
  reactivateStudentAfterCareError,
  terminateStudentAfterCarePending,
  terminateStudentAfterCareSuccess,
  terminateStudentAfterCareError,
} from "./actions";

export const StudentAfterCareProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(StudentAfterCareReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getStudentAfterCarePending());
    const endpoint = `/api/services/app/StudentAfterCare/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getStudentAfterCareSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getStudentAfterCareError());
      });
  };

  const getAllAsync = async (input?: IGetStudentAfterCaresInput) => {
    dispatch(getAllStudentAfterCaresPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.studentId) params.append('StudentId', input.studentId);
    if (input?.afterCareId) params.append('AfterCareId', input.afterCareId);
    if (input?.academicYearId) params.append('AcademicYearId', input.academicYearId);
    if (input?.status !== undefined && input?.status !== null) params.append('Status', input.status.toString());

    const endpoint = `/api/services/app/StudentAfterCare/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllStudentAfterCaresSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllStudentAfterCaresError());
      });
  };

  const getByAfterCareAsync = async (afterCareId: string) => {
    dispatch(getByAfterCarePending());
    const endpoint = `/api/services/app/StudentAfterCare/GetByAfterCare?afterCareId=${afterCareId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByAfterCareSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByAfterCareError());
      });
  };

  const getByStudentAsync = async (studentId: string) => {
    dispatch(getByStudentPending());
    const endpoint = `/api/services/app/StudentAfterCare/GetByStudent?studentId=${studentId}`;
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

  const createAsync = async (input: ICreateStudentAfterCare) => {
    dispatch(createStudentAfterCarePending());
    const endpoint = `/api/services/app/StudentAfterCare/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createStudentAfterCareSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createStudentAfterCareError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateStudentAfterCare) => {
    dispatch(updateStudentAfterCarePending());
    const endpoint = `/api/services/app/StudentAfterCare/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(updateStudentAfterCareSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateStudentAfterCareError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteStudentAfterCarePending());
    const endpoint = `/api/services/app/StudentAfterCare/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteStudentAfterCareSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteStudentAfterCareError());
      });
  };

  const suspendAsync = async (id: string) => {
    dispatch(suspendStudentAfterCarePending());
    const endpoint = `/api/services/app/StudentAfterCare/Suspend`;
    await instance
      .post(endpoint, { id })
      .then((response) => {
        dispatch(suspendStudentAfterCareSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(suspendStudentAfterCareError());
      });
  };

  const reactivateAsync = async (id: string) => {
    dispatch(reactivateStudentAfterCarePending());
    const endpoint = `/api/services/app/StudentAfterCare/Reactivate`;
    await instance
      .post(endpoint, { id })
      .then((response) => {
        dispatch(reactivateStudentAfterCareSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(reactivateStudentAfterCareError());
      });
  };

  const terminateAsync = async (id: string) => {
    dispatch(terminateStudentAfterCarePending());
    const endpoint = `/api/services/app/StudentAfterCare/Terminate`;
    await instance
      .post(endpoint, { id })
      .then((response) => {
        dispatch(terminateStudentAfterCareSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(terminateStudentAfterCareError());
      });
  };

  return (
    <StudentAfterCareStateContext.Provider value={state}>
      <StudentAfterCareActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getByAfterCareAsync,
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
      </StudentAfterCareActionContext.Provider>
    </StudentAfterCareStateContext.Provider>
  );
};

export const useStudentAfterCareState = () => {
  const context = useContext(StudentAfterCareStateContext);
  if (!context) {
    throw new Error("useStudentAfterCareState must be used within a StudentAfterCareProvider");
  }
  return context;
};

export const useStudentAfterCareActions = () => {
  const context = useContext(StudentAfterCareActionContext);
  if (!context) {
    throw new Error("useStudentAfterCareActions must be used within a StudentAfterCareProvider");
  }
  return context;
};
