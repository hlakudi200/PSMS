import { createAction } from "redux-actions";
import { IApplicationStateContext } from "./context";
import {
  IApplication,
  IApplicationList,
  IApplicationStatistics,
  IPagedResult,
} from "../shared/interfaces";

export enum ApplicationActionEnums {
  getApplicationsPending = "GET_APPLICATIONS_PENDING",
  getApplicationsSuccess = "GET_APPLICATIONS_SUCCESS",
  getApplicationsError = "GET_APPLICATIONS_ERROR",

  getApplicationPending = "GET_APPLICATION_PENDING",
  getApplicationSuccess = "GET_APPLICATION_SUCCESS",
  getApplicationError = "GET_APPLICATION_ERROR",

  getApplicationByNumberPending = "GET_APPLICATION_BY_NUMBER_PENDING",
  getApplicationByNumberSuccess = "GET_APPLICATION_BY_NUMBER_SUCCESS",
  getApplicationByNumberError = "GET_APPLICATION_BY_NUMBER_ERROR",

  createApplicationPending = "CREATE_APPLICATION_PENDING",
  createApplicationSuccess = "CREATE_APPLICATION_SUCCESS",
  createApplicationError = "CREATE_APPLICATION_ERROR",

  updateApplicationPending = "UPDATE_APPLICATION_PENDING",
  updateApplicationSuccess = "UPDATE_APPLICATION_SUCCESS",
  updateApplicationError = "UPDATE_APPLICATION_ERROR",

  deleteApplicationPending = "DELETE_APPLICATION_PENDING",
  deleteApplicationSuccess = "DELETE_APPLICATION_SUCCESS",
  deleteApplicationError = "DELETE_APPLICATION_ERROR",

  submitApplicationPending = "SUBMIT_APPLICATION_PENDING",
  submitApplicationSuccess = "SUBMIT_APPLICATION_SUCCESS",
  submitApplicationError = "SUBMIT_APPLICATION_ERROR",

  withdrawApplicationPending = "WITHDRAW_APPLICATION_PENDING",
  withdrawApplicationSuccess = "WITHDRAW_APPLICATION_SUCCESS",
  withdrawApplicationError = "WITHDRAW_APPLICATION_ERROR",

  getStatisticsPending = "GET_APPLICATION_STATISTICS_PENDING",
  getStatisticsSuccess = "GET_APPLICATION_STATISTICS_SUCCESS",
  getStatisticsError = "GET_APPLICATION_STATISTICS_ERROR",
}

// Get All Applications Actions
export const getApplicationsPending = createAction<IApplicationStateContext>(
  ApplicationActionEnums.getApplicationsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getApplicationsSuccess = createAction<
  IApplicationStateContext,
  IPagedResult<IApplicationList>
>(
  ApplicationActionEnums.getApplicationsSuccess,
  (result: IPagedResult<IApplicationList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    applications: result.items,
    totalCount: result.totalCount,
  })
);

export const getApplicationsError = createAction<IApplicationStateContext>(
  ApplicationActionEnums.getApplicationsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Single Application Actions
export const getApplicationPending = createAction<IApplicationStateContext>(
  ApplicationActionEnums.getApplicationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getApplicationSuccess = createAction<IApplicationStateContext, IApplication>(
  ApplicationActionEnums.getApplicationSuccess,
  (application: IApplication) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    application,
  })
);

export const getApplicationError = createAction<IApplicationStateContext>(
  ApplicationActionEnums.getApplicationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Application By Number Actions
export const getApplicationByNumberPending = createAction<IApplicationStateContext>(
  ApplicationActionEnums.getApplicationByNumberPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getApplicationByNumberSuccess = createAction<IApplicationStateContext, IApplication>(
  ApplicationActionEnums.getApplicationByNumberSuccess,
  (application: IApplication) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    application,
  })
);

export const getApplicationByNumberError = createAction<IApplicationStateContext>(
  ApplicationActionEnums.getApplicationByNumberError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Application Actions
export const createApplicationPending = createAction<IApplicationStateContext>(
  ApplicationActionEnums.createApplicationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createApplicationSuccess = createAction<IApplicationStateContext, IApplication>(
  ApplicationActionEnums.createApplicationSuccess,
  (application: IApplication) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    application,
  })
);

export const createApplicationError = createAction<IApplicationStateContext>(
  ApplicationActionEnums.createApplicationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Application Actions
export const updateApplicationPending = createAction<IApplicationStateContext>(
  ApplicationActionEnums.updateApplicationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateApplicationSuccess = createAction<IApplicationStateContext, IApplication>(
  ApplicationActionEnums.updateApplicationSuccess,
  (application: IApplication) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    application,
  })
);

export const updateApplicationError = createAction<IApplicationStateContext>(
  ApplicationActionEnums.updateApplicationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Application Actions
export const deleteApplicationPending = createAction<IApplicationStateContext>(
  ApplicationActionEnums.deleteApplicationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteApplicationSuccess = createAction<IApplicationStateContext>(
  ApplicationActionEnums.deleteApplicationSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteApplicationError = createAction<IApplicationStateContext>(
  ApplicationActionEnums.deleteApplicationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Submit Application Actions
export const submitApplicationPending = createAction<IApplicationStateContext>(
  ApplicationActionEnums.submitApplicationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const submitApplicationSuccess = createAction<IApplicationStateContext, IApplication>(
  ApplicationActionEnums.submitApplicationSuccess,
  (application: IApplication) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    application,
  })
);

export const submitApplicationError = createAction<IApplicationStateContext>(
  ApplicationActionEnums.submitApplicationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Withdraw Application Actions
export const withdrawApplicationPending = createAction<IApplicationStateContext>(
  ApplicationActionEnums.withdrawApplicationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const withdrawApplicationSuccess = createAction<IApplicationStateContext, IApplication>(
  ApplicationActionEnums.withdrawApplicationSuccess,
  (application: IApplication) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    application,
  })
);

export const withdrawApplicationError = createAction<IApplicationStateContext>(
  ApplicationActionEnums.withdrawApplicationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Statistics Actions
export const getStatisticsPending = createAction<IApplicationStateContext>(
  ApplicationActionEnums.getStatisticsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getStatisticsSuccess = createAction<IApplicationStateContext, IApplicationStatistics>(
  ApplicationActionEnums.getStatisticsSuccess,
  (statistics: IApplicationStatistics) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    statistics,
  })
);

export const getStatisticsError = createAction<IApplicationStateContext>(
  ApplicationActionEnums.getStatisticsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
