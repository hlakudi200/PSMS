"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  AdmissionSettingsActionContext,
  AdmissionSettingsStateContext,
} from "./context";
import {
  ICreateAdmissionSettings,
  IUpdateAdmissionSettings,
} from "../shared/interfaces";
import { AdmissionSettingsReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getAdmissionSettingsPending,
  getAdmissionSettingsSuccess,
  getAdmissionSettingsError,
  getAdmissionSettingsByGradePending,
  getAdmissionSettingsByGradeSuccess,
  getAdmissionSettingsByGradeError,
  getAllByAcademicYearPending,
  getAllByAcademicYearSuccess,
  getAllByAcademicYearError,
  createAdmissionSettingsPending,
  createAdmissionSettingsSuccess,
  createAdmissionSettingsError,
  updateAdmissionSettingsPending,
  updateAdmissionSettingsSuccess,
  updateAdmissionSettingsError,
  deleteAdmissionSettingsPending,
  deleteAdmissionSettingsSuccess,
  deleteAdmissionSettingsError,
  getCapacityStatusPending,
  getCapacityStatusSuccess,
  getCapacityStatusError,
  openApplicationsPending,
  openApplicationsSuccess,
  openApplicationsError,
  closeApplicationsPending,
  closeApplicationsSuccess,
  closeApplicationsError,
  updateCapacityPending,
  updateCapacitySuccess,
  updateCapacityError,
} from "./actions";

export const AdmissionSettingsProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(AdmissionSettingsReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getAdmissionSettingsPending());
    const endpoint = `/api/services/app/AdmissionSettings/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAdmissionSettingsSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAdmissionSettingsError());
      });
  };

  const getByGradeAsync = async (academicYearId: string, gradeId: string) => {
    dispatch(getAdmissionSettingsByGradePending());
    const endpoint = `/api/services/app/AdmissionSettings/GetByGrade?academicYearId=${academicYearId}&gradeId=${gradeId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAdmissionSettingsByGradeSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAdmissionSettingsByGradeError());
      });
  };

  const getAllByAcademicYearAsync = async (academicYearId: string) => {
    dispatch(getAllByAcademicYearPending());
    const endpoint = `/api/services/app/AdmissionSettings/GetAllByAcademicYear?academicYearId=${academicYearId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllByAcademicYearSuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllByAcademicYearError());
      });
  };

  const createAsync = async (input: ICreateAdmissionSettings) => {
    dispatch(createAdmissionSettingsPending());
    const endpoint = `/api/services/app/AdmissionSettings/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createAdmissionSettingsSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createAdmissionSettingsError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateAdmissionSettings) => {
    dispatch(updateAdmissionSettingsPending());
    const endpoint = `/api/services/app/AdmissionSettings/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateAdmissionSettingsSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateAdmissionSettingsError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteAdmissionSettingsPending());
    const endpoint = `/api/services/app/AdmissionSettings/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteAdmissionSettingsSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteAdmissionSettingsError());
      });
  };

  const getCapacityStatusAsync = async (academicYearId: string, gradeId: string) => {
    dispatch(getCapacityStatusPending());
    const endpoint = `/api/services/app/AdmissionSettings/GetCapacityStatus?academicYearId=${academicYearId}&gradeId=${gradeId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getCapacityStatusSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getCapacityStatusError());
      });
  };

  const openApplicationsAsync = async (academicYearId: string, gradeId?: string) => {
    dispatch(openApplicationsPending());
    const params = new URLSearchParams();
    params.append('academicYearId', academicYearId);
    if (gradeId) params.append('gradeId', gradeId);
    const endpoint = `/api/services/app/AdmissionSettings/OpenApplications?${params.toString()}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(openApplicationsSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(openApplicationsError());
      });
  };

  const closeApplicationsAsync = async (academicYearId: string, gradeId?: string) => {
    dispatch(closeApplicationsPending());
    const params = new URLSearchParams();
    params.append('academicYearId', academicYearId);
    if (gradeId) params.append('gradeId', gradeId);
    const endpoint = `/api/services/app/AdmissionSettings/CloseApplications?${params.toString()}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(closeApplicationsSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(closeApplicationsError());
      });
  };

  const updateCapacityAsync = async (academicYearId: string, gradeId: string, newCapacity: number) => {
    dispatch(updateCapacityPending());
    const endpoint = `/api/services/app/AdmissionSettings/UpdateCapacity?academicYearId=${academicYearId}&gradeId=${gradeId}&newCapacity=${newCapacity}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(updateCapacitySuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateCapacityError());
      });
  };

  return (
    <AdmissionSettingsStateContext.Provider value={state}>
      <AdmissionSettingsActionContext.Provider
        value={{
          getAsync,
          getByGradeAsync,
          getAllByAcademicYearAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          getCapacityStatusAsync,
          openApplicationsAsync,
          closeApplicationsAsync,
          updateCapacityAsync,
        }}
      >
        {children}
      </AdmissionSettingsActionContext.Provider>
    </AdmissionSettingsStateContext.Provider>
  );
};

export const useAdmissionSettingsState = () => {
  const context = useContext(AdmissionSettingsStateContext);
  if (!context) {
    throw new Error("useAdmissionSettingsState must be used within an AdmissionSettingsProvider");
  }
  return context;
};

export const useAdmissionSettingsActions = () => {
  const context = useContext(AdmissionSettingsActionContext);
  if (!context) {
    throw new Error("useAdmissionSettingsActions must be used within an AdmissionSettingsProvider");
  }
  return context;
};
