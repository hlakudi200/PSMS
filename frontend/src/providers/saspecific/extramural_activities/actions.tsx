import { createAction } from "redux-actions";
import { IExtramuralActivityStateContext } from "./context";
import { IExtramuralActivity, IExtramuralActivityList, IPagedResult, IListResult } from "../shared/interfaces";

export enum ExtramuralActivityActionEnums {
  getExtramuralActivityPending = "GET_EXTRAMURAL_ACTIVITY_PENDING",
  getExtramuralActivitySuccess = "GET_EXTRAMURAL_ACTIVITY_SUCCESS",
  getExtramuralActivityError = "GET_EXTRAMURAL_ACTIVITY_ERROR",

  getAllExtramuralActivitiesPending = "GET_ALL_EXTRAMURAL_ACTIVITIES_PENDING",
  getAllExtramuralActivitiesSuccess = "GET_ALL_EXTRAMURAL_ACTIVITIES_SUCCESS",
  getAllExtramuralActivitiesError = "GET_ALL_EXTRAMURAL_ACTIVITIES_ERROR",

  getByAcademicYearPending = "GET_EXTRAMURAL_ACTIVITIES_BY_ACADEMIC_YEAR_PENDING",
  getByAcademicYearSuccess = "GET_EXTRAMURAL_ACTIVITIES_BY_ACADEMIC_YEAR_SUCCESS",
  getByAcademicYearError = "GET_EXTRAMURAL_ACTIVITIES_BY_ACADEMIC_YEAR_ERROR",

  createExtramuralActivityPending = "CREATE_EXTRAMURAL_ACTIVITY_PENDING",
  createExtramuralActivitySuccess = "CREATE_EXTRAMURAL_ACTIVITY_SUCCESS",
  createExtramuralActivityError = "CREATE_EXTRAMURAL_ACTIVITY_ERROR",

  updateExtramuralActivityPending = "UPDATE_EXTRAMURAL_ACTIVITY_PENDING",
  updateExtramuralActivitySuccess = "UPDATE_EXTRAMURAL_ACTIVITY_SUCCESS",
  updateExtramuralActivityError = "UPDATE_EXTRAMURAL_ACTIVITY_ERROR",

  deleteExtramuralActivityPending = "DELETE_EXTRAMURAL_ACTIVITY_PENDING",
  deleteExtramuralActivitySuccess = "DELETE_EXTRAMURAL_ACTIVITY_SUCCESS",
  deleteExtramuralActivityError = "DELETE_EXTRAMURAL_ACTIVITY_ERROR",

  activateExtramuralActivityPending = "ACTIVATE_EXTRAMURAL_ACTIVITY_PENDING",
  activateExtramuralActivitySuccess = "ACTIVATE_EXTRAMURAL_ACTIVITY_SUCCESS",
  activateExtramuralActivityError = "ACTIVATE_EXTRAMURAL_ACTIVITY_ERROR",

  deactivateExtramuralActivityPending = "DEACTIVATE_EXTRAMURAL_ACTIVITY_PENDING",
  deactivateExtramuralActivitySuccess = "DEACTIVATE_EXTRAMURAL_ACTIVITY_SUCCESS",
  deactivateExtramuralActivityError = "DEACTIVATE_EXTRAMURAL_ACTIVITY_ERROR",

  openRegistrationPending = "OPEN_REGISTRATION_EXTRAMURAL_ACTIVITY_PENDING",
  openRegistrationSuccess = "OPEN_REGISTRATION_EXTRAMURAL_ACTIVITY_SUCCESS",
  openRegistrationError = "OPEN_REGISTRATION_EXTRAMURAL_ACTIVITY_ERROR",

  closeRegistrationPending = "CLOSE_REGISTRATION_EXTRAMURAL_ACTIVITY_PENDING",
  closeRegistrationSuccess = "CLOSE_REGISTRATION_EXTRAMURAL_ACTIVITY_SUCCESS",
  closeRegistrationError = "CLOSE_REGISTRATION_EXTRAMURAL_ACTIVITY_ERROR",
}

// Get Single ExtramuralActivity Actions
export const getExtramuralActivityPending = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.getExtramuralActivityPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getExtramuralActivitySuccess = createAction<IExtramuralActivityStateContext, IExtramuralActivity>(
  ExtramuralActivityActionEnums.getExtramuralActivitySuccess,
  (extramuralActivity: IExtramuralActivity) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    extramuralActivity,
  })
);

export const getExtramuralActivityError = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.getExtramuralActivityError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All ExtramuralActivities Actions
export const getAllExtramuralActivitiesPending = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.getAllExtramuralActivitiesPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllExtramuralActivitiesSuccess = createAction<
  IExtramuralActivityStateContext,
  IPagedResult<IExtramuralActivityList>
>(
  ExtramuralActivityActionEnums.getAllExtramuralActivitiesSuccess,
  (result: IPagedResult<IExtramuralActivityList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    extramuralActivities: result.items,
    totalCount: result.totalCount,
  })
);

export const getAllExtramuralActivitiesError = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.getAllExtramuralActivitiesError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By AcademicYear Actions
export const getByAcademicYearPending = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.getByAcademicYearPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByAcademicYearSuccess = createAction<
  IExtramuralActivityStateContext,
  IListResult<IExtramuralActivityList>
>(
  ExtramuralActivityActionEnums.getByAcademicYearSuccess,
  (result: IListResult<IExtramuralActivityList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    extramuralActivities: result.items,
  })
);

export const getByAcademicYearError = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.getByAcademicYearError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create ExtramuralActivity Actions
export const createExtramuralActivityPending = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.createExtramuralActivityPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createExtramuralActivitySuccess = createAction<IExtramuralActivityStateContext, IExtramuralActivity>(
  ExtramuralActivityActionEnums.createExtramuralActivitySuccess,
  (extramuralActivity: IExtramuralActivity) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    extramuralActivity,
  })
);

export const createExtramuralActivityError = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.createExtramuralActivityError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update ExtramuralActivity Actions
export const updateExtramuralActivityPending = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.updateExtramuralActivityPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateExtramuralActivitySuccess = createAction<IExtramuralActivityStateContext, IExtramuralActivity>(
  ExtramuralActivityActionEnums.updateExtramuralActivitySuccess,
  (extramuralActivity: IExtramuralActivity) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    extramuralActivity,
  })
);

export const updateExtramuralActivityError = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.updateExtramuralActivityError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete ExtramuralActivity Actions
export const deleteExtramuralActivityPending = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.deleteExtramuralActivityPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteExtramuralActivitySuccess = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.deleteExtramuralActivitySuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteExtramuralActivityError = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.deleteExtramuralActivityError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Activate ExtramuralActivity Actions
export const activateExtramuralActivityPending = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.activateExtramuralActivityPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const activateExtramuralActivitySuccess = createAction<IExtramuralActivityStateContext, IExtramuralActivity>(
  ExtramuralActivityActionEnums.activateExtramuralActivitySuccess,
  (extramuralActivity: IExtramuralActivity) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    extramuralActivity,
  })
);

export const activateExtramuralActivityError = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.activateExtramuralActivityError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Deactivate ExtramuralActivity Actions
export const deactivateExtramuralActivityPending = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.deactivateExtramuralActivityPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deactivateExtramuralActivitySuccess = createAction<IExtramuralActivityStateContext, IExtramuralActivity>(
  ExtramuralActivityActionEnums.deactivateExtramuralActivitySuccess,
  (extramuralActivity: IExtramuralActivity) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    extramuralActivity,
  })
);

export const deactivateExtramuralActivityError = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.deactivateExtramuralActivityError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Open Registration Actions
export const openRegistrationPending = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.openRegistrationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const openRegistrationSuccess = createAction<IExtramuralActivityStateContext, IExtramuralActivity>(
  ExtramuralActivityActionEnums.openRegistrationSuccess,
  (extramuralActivity: IExtramuralActivity) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    extramuralActivity,
  })
);

export const openRegistrationError = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.openRegistrationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Close Registration Actions
export const closeRegistrationPending = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.closeRegistrationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const closeRegistrationSuccess = createAction<IExtramuralActivityStateContext, IExtramuralActivity>(
  ExtramuralActivityActionEnums.closeRegistrationSuccess,
  (extramuralActivity: IExtramuralActivity) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    extramuralActivity,
  })
);

export const closeRegistrationError = createAction<IExtramuralActivityStateContext>(
  ExtramuralActivityActionEnums.closeRegistrationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
