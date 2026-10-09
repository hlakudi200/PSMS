"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  AdmissionSettingsActionContext,
  AdmissionSettingsStateContext,
} from "./context";
import {
  IAdmissionSettings,
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
        throw error;
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
        throw error;
      });
  };

  /**
   * Every settings row the school has, narrowed by year and grade when the
   * screen's filters are set. Reuses the by-year slot in state, since it is
   * the same list read a different way.
   */
  /**
   * What a prospective parent may apply for. Lives on the Application service,
   * not this one: ABP checks the class-level permission as well as the
   * method's, and AdmissionSettingsAppService is guarded by a permission no
   * applicant holds.
   *
   * Returned rather than dispatched: the apply screen is the only caller and
   * wants it inline.
   */
  const getOpenIntakesAsync = async () => {
    const response = await instance.get('/api/services/app/Application/GetOpenIntakes');
    return response.data?.result?.items ?? [];
  };

  const getAllAsync = async (academicYearId?: string, gradeId?: string) => {
    dispatch(getAllByAcademicYearPending());
    const params = new URLSearchParams();
    if (academicYearId) params.set('academicYearId', academicYearId);
    if (gradeId) params.set('gradeId', gradeId);
    const query = params.toString();
    const endpoint = `/api/services/app/AdmissionSettings/GetAll${query ? `?${query}` : ''}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllByAcademicYearSuccess({ items: response.data.result.items }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllByAcademicYearError());
        throw error;
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
        throw error;
      });
  };

  /**
   * Returns every row written, which is one per grade when no grade was
   * chosen — "every grade" writes real settings for each rather than one row
   * nobody can apply to.
   */
  const createAsync = async (input: ICreateAdmissionSettings): Promise<IAdmissionSettings[]> => {
    dispatch(createAdmissionSettingsPending());
    const endpoint = `/api/services/app/AdmissionSettings/Create`;
    return instance
      .post(endpoint, input)
      .then((response) => {
        const items: IAdmissionSettings[] = response.data.result.items ?? [];
        dispatch(createAdmissionSettingsSuccess(items[0]));
        return items;
      })
      .catch((error) => {
        console.error(error);
        dispatch(createAdmissionSettingsError());
        throw error;
      });
  };

  /** Turns a leftover year-wide row into the per-grade settings it stood for. */
  const expandToEveryGradeAsync = async (id: string): Promise<IAdmissionSettings[]> => {
    const endpoint = `/api/services/app/AdmissionSettings/ExpandToEveryGrade?id=${id}`;
    return instance
      .post(endpoint)
      .then((response) => response.data.result.items ?? [])
      .catch((error) => {
        console.error(error);
        throw error;
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
        throw error;
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
        throw error;
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
        throw error;
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
        throw error;
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
        throw error;
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
        throw error;
      });
  };

  return (
    <AdmissionSettingsStateContext.Provider value={state}>
      <AdmissionSettingsActionContext.Provider
        value={{
          getAsync,
          getByGradeAsync,
          getAllAsync,
          getOpenIntakesAsync,
          getAllByAcademicYearAsync,
          createAsync,
          expandToEveryGradeAsync,
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
