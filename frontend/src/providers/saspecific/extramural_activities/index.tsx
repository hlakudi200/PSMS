"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  ExtramuralActivityActionContext,
  ExtramuralActivityStateContext,
} from "./context";
import {
  ICreateExtramuralActivity,
  IUpdateExtramuralActivity,
  IGetExtramuralActivitiesInput,
} from "../shared/interfaces";
import { ExtramuralActivityReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getExtramuralActivityPending,
  getExtramuralActivitySuccess,
  getExtramuralActivityError,
  getAllExtramuralActivitiesPending,
  getAllExtramuralActivitiesSuccess,
  getAllExtramuralActivitiesError,
  getByAcademicYearPending,
  getByAcademicYearSuccess,
  getByAcademicYearError,
  createExtramuralActivityPending,
  createExtramuralActivitySuccess,
  createExtramuralActivityError,
  updateExtramuralActivityPending,
  updateExtramuralActivitySuccess,
  updateExtramuralActivityError,
  deleteExtramuralActivityPending,
  deleteExtramuralActivitySuccess,
  deleteExtramuralActivityError,
  activateExtramuralActivityPending,
  activateExtramuralActivitySuccess,
  activateExtramuralActivityError,
  deactivateExtramuralActivityPending,
  deactivateExtramuralActivitySuccess,
  deactivateExtramuralActivityError,
  openRegistrationPending,
  openRegistrationSuccess,
  openRegistrationError,
  closeRegistrationPending,
  closeRegistrationSuccess,
  closeRegistrationError,
} from "./actions";

export const ExtramuralActivityProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(ExtramuralActivityReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getExtramuralActivityPending());
    const endpoint = `/api/services/app/ExtramuralActivity/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getExtramuralActivitySuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getExtramuralActivityError());
      });
  };

  const getAllAsync = async (input?: IGetExtramuralActivitiesInput) => {
    dispatch(getAllExtramuralActivitiesPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.academicYearId) params.append('AcademicYearId', input.academicYearId);
    if (input?.category !== undefined && input?.category !== null) params.append('Category', input.category.toString());
    if (input?.activityType !== undefined && input?.activityType !== null) params.append('ActivityType', input.activityType.toString());
    if (input?.isActive !== undefined && input?.isActive !== null) params.append('IsActive', input.isActive.toString());
    if (input?.isRegistrationOpen !== undefined && input?.isRegistrationOpen !== null) params.append('IsRegistrationOpen', input.isRegistrationOpen.toString());
    if (input?.activityName) params.append('ActivityName', input.activityName);

    const endpoint = `/api/services/app/ExtramuralActivity/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllExtramuralActivitiesSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllExtramuralActivitiesError());
      });
  };

  const getByAcademicYearAsync = async (academicYearId: string) => {
    dispatch(getByAcademicYearPending());
    const endpoint = `/api/services/app/ExtramuralActivity/GetByAcademicYear?academicYearId=${academicYearId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByAcademicYearSuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByAcademicYearError());
      });
  };

  const createAsync = async (input: ICreateExtramuralActivity) => {
    dispatch(createExtramuralActivityPending());
    const endpoint = `/api/services/app/ExtramuralActivity/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createExtramuralActivitySuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createExtramuralActivityError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateExtramuralActivity) => {
    dispatch(updateExtramuralActivityPending());
    const endpoint = `/api/services/app/ExtramuralActivity/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateExtramuralActivitySuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateExtramuralActivityError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteExtramuralActivityPending());
    const endpoint = `/api/services/app/ExtramuralActivity/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteExtramuralActivitySuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteExtramuralActivityError());
      });
  };

  const activateAsync = async (id: string) => {
    dispatch(activateExtramuralActivityPending());
    const endpoint = `/api/services/app/ExtramuralActivity/Activate?id=${id}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(activateExtramuralActivitySuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(activateExtramuralActivityError());
      });
  };

  const deactivateAsync = async (id: string) => {
    dispatch(deactivateExtramuralActivityPending());
    const endpoint = `/api/services/app/ExtramuralActivity/Deactivate?id=${id}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(deactivateExtramuralActivitySuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(deactivateExtramuralActivityError());
      });
  };

  const openRegistrationAsync = async (id: string) => {
    dispatch(openRegistrationPending());
    const endpoint = `/api/services/app/ExtramuralActivity/OpenRegistration?id=${id}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(openRegistrationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(openRegistrationError());
      });
  };

  const closeRegistrationAsync = async (id: string) => {
    dispatch(closeRegistrationPending());
    const endpoint = `/api/services/app/ExtramuralActivity/CloseRegistration?id=${id}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(closeRegistrationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(closeRegistrationError());
      });
  };

  return (
    <ExtramuralActivityStateContext.Provider value={state}>
      <ExtramuralActivityActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getByAcademicYearAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          activateAsync,
          deactivateAsync,
          openRegistrationAsync,
          closeRegistrationAsync,
        }}
      >
        {children}
      </ExtramuralActivityActionContext.Provider>
    </ExtramuralActivityStateContext.Provider>
  );
};

export const useExtramuralActivityState = () => {
  const context = useContext(ExtramuralActivityStateContext);
  if (!context) {
    throw new Error("useExtramuralActivityState must be used within an ExtramuralActivityProvider");
  }
  return context;
};

export const useExtramuralActivityActions = () => {
  const context = useContext(ExtramuralActivityActionContext);
  if (!context) {
    throw new Error("useExtramuralActivityActions must be used within an ExtramuralActivityProvider");
  }
  return context;
};
