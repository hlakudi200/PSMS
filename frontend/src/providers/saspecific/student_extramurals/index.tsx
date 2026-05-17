"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  StudentExtramuralActionContext,
  StudentExtramuralStateContext,
} from "./context";
import {
  ICreateStudentExtramural,
  IUpdateStudentExtramural,
  IGetStudentExtramuralsInput,
} from "../shared/interfaces";
import { StudentExtramuralReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getStudentExtramuralPending,
  getStudentExtramuralSuccess,
  getStudentExtramuralError,
  getStudentExtramuralsPending,
  getStudentExtramuralsSuccess,
  getStudentExtramuralsError,
  getByActivityPending,
  getByActivitySuccess,
  getByActivityError,
  getByStudentPending,
  getByStudentSuccess,
  getByStudentError,
  createStudentExtramuralPending,
  createStudentExtramuralSuccess,
  createStudentExtramuralError,
  updateStudentExtramuralPending,
  updateStudentExtramuralSuccess,
  updateStudentExtramuralError,
  deleteStudentExtramuralPending,
  deleteStudentExtramuralSuccess,
  deleteStudentExtramuralError,
  suspendPending,
  suspendSuccess,
  suspendError,
  reactivatePending,
  reactivateSuccess,
  reactivateError,
  terminatePending,
  terminateSuccess,
  terminateError,
  signConsentFormPending,
  signConsentFormSuccess,
  signConsentFormError,
} from "./actions";

export const StudentExtramuralProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(StudentExtramuralReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getStudentExtramuralPending());
    const endpoint = `/api/services/app/StudentExtramural/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getStudentExtramuralSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getStudentExtramuralError());
        throw error;
      });
  };

  const getAllAsync = async (input?: IGetStudentExtramuralsInput) => {
    dispatch(getStudentExtramuralsPending());
    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append("MaxResultCount", input.maxResultCount.toString());
    if (input?.skipCount) params.append("SkipCount", input.skipCount.toString());
    if (input?.sorting) params.append("Sorting", input.sorting);
    if (input?.studentId) params.append("StudentId", input.studentId);
    if (input?.extramuralActivityId) params.append("ExtramuralActivityId", input.extramuralActivityId);
    if (input?.academicYearId) params.append("AcademicYearId", input.academicYearId);
    if (input?.status !== undefined) params.append("Status", input.status.toString());
    const endpoint = `/api/services/app/StudentExtramural/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getStudentExtramuralsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getStudentExtramuralsError());
        throw error;
      });
  };

  const getByActivityAsync = async (activityId: string) => {
    dispatch(getByActivityPending());
    const endpoint = `/api/services/app/StudentExtramural/GetByActivity?activityId=${activityId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByActivitySuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByActivityError());
        throw error;
      });
  };

  const getByStudentAsync = async (studentId: string) => {
    dispatch(getByStudentPending());
    const endpoint = `/api/services/app/StudentExtramural/GetByStudent?studentId=${studentId}`;
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

  const createAsync = async (input: ICreateStudentExtramural) => {
    dispatch(createStudentExtramuralPending());
    const endpoint = `/api/services/app/StudentExtramural/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createStudentExtramuralSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createStudentExtramuralError());
        throw error;
      });
  };

  const updateAsync = async (id: string, input: IUpdateStudentExtramural) => {
    dispatch(updateStudentExtramuralPending());
    const endpoint = `/api/services/app/StudentExtramural/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateStudentExtramuralSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateStudentExtramuralError());
        throw error;
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteStudentExtramuralPending());
    const endpoint = `/api/services/app/StudentExtramural/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteStudentExtramuralSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteStudentExtramuralError());
        throw error;
      });
  };

  const suspendAsync = async (id: string) => {
    dispatch(suspendPending());
    const endpoint = `/api/services/app/StudentExtramural/Suspend?id=${id}`;
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
    const endpoint = `/api/services/app/StudentExtramural/Reactivate?id=${id}`;
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
    const endpoint = `/api/services/app/StudentExtramural/Terminate?id=${id}`;
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

  const signConsentFormAsync = async (id: string) => {
    dispatch(signConsentFormPending());
    const endpoint = `/api/services/app/StudentExtramural/SignConsentForm?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(signConsentFormSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(signConsentFormError());
        throw error;
      });
  };

  return (
    <StudentExtramuralStateContext.Provider value={state}>
      <StudentExtramuralActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getByActivityAsync,
          getByStudentAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          suspendAsync,
          reactivateAsync,
          terminateAsync,
          signConsentFormAsync,
        }}
      >
        {children}
      </StudentExtramuralActionContext.Provider>
    </StudentExtramuralStateContext.Provider>
  );
};

export const useStudentExtramuralState = () => {
  const context = useContext(StudentExtramuralStateContext);
  if (!context) {
    throw new Error("useStudentExtramuralState must be used within a StudentExtramuralProvider");
  }
  return context;
};

export const useStudentExtramuralActions = () => {
  const context = useContext(StudentExtramuralActionContext);
  if (!context) {
    throw new Error("useStudentExtramuralActions must be used within a StudentExtramuralProvider");
  }
  return context;
};
