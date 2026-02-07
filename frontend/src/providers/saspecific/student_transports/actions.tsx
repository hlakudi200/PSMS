import { createAction } from "redux-actions";
import { IStudentTransportStateContext } from "./context";
import { IStudentTransport, IStudentTransportList, IPagedResult, IListResult } from "../shared/interfaces";

export enum StudentTransportActionEnums {
  getStudentTransportPending = "GET_STUDENT_TRANSPORT_PENDING",
  getStudentTransportSuccess = "GET_STUDENT_TRANSPORT_SUCCESS",
  getStudentTransportError = "GET_STUDENT_TRANSPORT_ERROR",

  getStudentTransportsPending = "GET_STUDENT_TRANSPORTS_PENDING",
  getStudentTransportsSuccess = "GET_STUDENT_TRANSPORTS_SUCCESS",
  getStudentTransportsError = "GET_STUDENT_TRANSPORTS_ERROR",

  getByTransportPending = "GET_STUDENT_TRANSPORTS_BY_TRANSPORT_PENDING",
  getByTransportSuccess = "GET_STUDENT_TRANSPORTS_BY_TRANSPORT_SUCCESS",
  getByTransportError = "GET_STUDENT_TRANSPORTS_BY_TRANSPORT_ERROR",

  getByStudentPending = "GET_STUDENT_TRANSPORTS_BY_STUDENT_PENDING",
  getByStudentSuccess = "GET_STUDENT_TRANSPORTS_BY_STUDENT_SUCCESS",
  getByStudentError = "GET_STUDENT_TRANSPORTS_BY_STUDENT_ERROR",

  createStudentTransportPending = "CREATE_STUDENT_TRANSPORT_PENDING",
  createStudentTransportSuccess = "CREATE_STUDENT_TRANSPORT_SUCCESS",
  createStudentTransportError = "CREATE_STUDENT_TRANSPORT_ERROR",

  updateStudentTransportPending = "UPDATE_STUDENT_TRANSPORT_PENDING",
  updateStudentTransportSuccess = "UPDATE_STUDENT_TRANSPORT_SUCCESS",
  updateStudentTransportError = "UPDATE_STUDENT_TRANSPORT_ERROR",

  deleteStudentTransportPending = "DELETE_STUDENT_TRANSPORT_PENDING",
  deleteStudentTransportSuccess = "DELETE_STUDENT_TRANSPORT_SUCCESS",
  deleteStudentTransportError = "DELETE_STUDENT_TRANSPORT_ERROR",

  suspendPending = "SUSPEND_STUDENT_TRANSPORT_PENDING",
  suspendSuccess = "SUSPEND_STUDENT_TRANSPORT_SUCCESS",
  suspendError = "SUSPEND_STUDENT_TRANSPORT_ERROR",

  reactivatePending = "REACTIVATE_STUDENT_TRANSPORT_PENDING",
  reactivateSuccess = "REACTIVATE_STUDENT_TRANSPORT_SUCCESS",
  reactivateError = "REACTIVATE_STUDENT_TRANSPORT_ERROR",

  terminatePending = "TERMINATE_STUDENT_TRANSPORT_PENDING",
  terminateSuccess = "TERMINATE_STUDENT_TRANSPORT_SUCCESS",
  terminateError = "TERMINATE_STUDENT_TRANSPORT_ERROR",
}

// Get Single StudentTransport
export const getStudentTransportPending = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.getStudentTransportPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getStudentTransportSuccess = createAction<IStudentTransportStateContext, IStudentTransport>(
  StudentTransportActionEnums.getStudentTransportSuccess,
  (studentTransport: IStudentTransport) => ({
    isPending: false, isSuccess: true, isError: false, studentTransport,
  })
);
export const getStudentTransportError = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.getStudentTransportError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All StudentTransports (paged)
export const getStudentTransportsPending = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.getStudentTransportsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getStudentTransportsSuccess = createAction<IStudentTransportStateContext, IPagedResult<IStudentTransportList>>(
  StudentTransportActionEnums.getStudentTransportsSuccess,
  (result: IPagedResult<IStudentTransportList>) => ({
    isPending: false, isSuccess: true, isError: false,
    studentTransports: result.items, totalCount: result.totalCount,
  })
);
export const getStudentTransportsError = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.getStudentTransportsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Transport
export const getByTransportPending = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.getByTransportPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getByTransportSuccess = createAction<IStudentTransportStateContext, IListResult<IStudentTransportList>>(
  StudentTransportActionEnums.getByTransportSuccess,
  (result: IListResult<IStudentTransportList>) => ({
    isPending: false, isSuccess: true, isError: false,
    studentTransports: result.items,
  })
);
export const getByTransportError = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.getByTransportError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Student
export const getByStudentPending = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.getByStudentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getByStudentSuccess = createAction<IStudentTransportStateContext, IListResult<IStudentTransportList>>(
  StudentTransportActionEnums.getByStudentSuccess,
  (result: IListResult<IStudentTransportList>) => ({
    isPending: false, isSuccess: true, isError: false,
    studentTransports: result.items,
  })
);
export const getByStudentError = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.getByStudentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create StudentTransport
export const createStudentTransportPending = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.createStudentTransportPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const createStudentTransportSuccess = createAction<IStudentTransportStateContext, IStudentTransport>(
  StudentTransportActionEnums.createStudentTransportSuccess,
  (studentTransport: IStudentTransport) => ({
    isPending: false, isSuccess: true, isError: false, studentTransport,
  })
);
export const createStudentTransportError = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.createStudentTransportError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update StudentTransport
export const updateStudentTransportPending = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.updateStudentTransportPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const updateStudentTransportSuccess = createAction<IStudentTransportStateContext, IStudentTransport>(
  StudentTransportActionEnums.updateStudentTransportSuccess,
  (studentTransport: IStudentTransport) => ({
    isPending: false, isSuccess: true, isError: false, studentTransport,
  })
);
export const updateStudentTransportError = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.updateStudentTransportError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete StudentTransport
export const deleteStudentTransportPending = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.deleteStudentTransportPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const deleteStudentTransportSuccess = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.deleteStudentTransportSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const deleteStudentTransportError = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.deleteStudentTransportError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Suspend StudentTransport
export const suspendPending = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.suspendPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const suspendSuccess = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.suspendSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const suspendError = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.suspendError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Reactivate StudentTransport
export const reactivatePending = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.reactivatePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const reactivateSuccess = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.reactivateSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const reactivateError = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.reactivateError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Terminate StudentTransport
export const terminatePending = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.terminatePending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const terminateSuccess = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.terminateSuccess,
  () => ({ isPending: false, isSuccess: true, isError: false })
);
export const terminateError = createAction<IStudentTransportStateContext>(
  StudentTransportActionEnums.terminateError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
