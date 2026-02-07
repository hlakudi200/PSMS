import { createAction } from "redux-actions";
import { ISchoolTransportStateContext } from "./context";
import { ISchoolTransport, ISchoolTransportList, IPagedResult } from "../shared/interfaces";

export enum SchoolTransportActionEnums {
    getSchoolTransportPending = "GET_SCHOOL_TRANSPORT_PENDING",
    getSchoolTransportSuccess = "GET_SCHOOL_TRANSPORT_SUCCESS",
    getSchoolTransportError = "GET_SCHOOL_TRANSPORT_ERROR",

    getAllSchoolTransportsPending = "GET_ALL_SCHOOL_TRANSPORTS_PENDING",
    getAllSchoolTransportsSuccess = "GET_ALL_SCHOOL_TRANSPORTS_SUCCESS",
    getAllSchoolTransportsError = "GET_ALL_SCHOOL_TRANSPORTS_ERROR",

    createSchoolTransportPending = "CREATE_SCHOOL_TRANSPORT_PENDING",
    createSchoolTransportSuccess = "CREATE_SCHOOL_TRANSPORT_SUCCESS",
    createSchoolTransportError = "CREATE_SCHOOL_TRANSPORT_ERROR",

    updateSchoolTransportPending = "UPDATE_SCHOOL_TRANSPORT_PENDING",
    updateSchoolTransportSuccess = "UPDATE_SCHOOL_TRANSPORT_SUCCESS",
    updateSchoolTransportError = "UPDATE_SCHOOL_TRANSPORT_ERROR",

    deleteSchoolTransportPending = "DELETE_SCHOOL_TRANSPORT_PENDING",
    deleteSchoolTransportSuccess = "DELETE_SCHOOL_TRANSPORT_SUCCESS",
    deleteSchoolTransportError = "DELETE_SCHOOL_TRANSPORT_ERROR",

    activateSchoolTransportPending = "ACTIVATE_SCHOOL_TRANSPORT_PENDING",
    activateSchoolTransportSuccess = "ACTIVATE_SCHOOL_TRANSPORT_SUCCESS",
    activateSchoolTransportError = "ACTIVATE_SCHOOL_TRANSPORT_ERROR",

    deactivateSchoolTransportPending = "DEACTIVATE_SCHOOL_TRANSPORT_PENDING",
    deactivateSchoolTransportSuccess = "DEACTIVATE_SCHOOL_TRANSPORT_SUCCESS",
    deactivateSchoolTransportError = "DEACTIVATE_SCHOOL_TRANSPORT_ERROR",
}

// Get Single SchoolTransport Actions
export const getSchoolTransportPending = createAction<ISchoolTransportStateContext>(
    SchoolTransportActionEnums.getSchoolTransportPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getSchoolTransportSuccess = createAction<ISchoolTransportStateContext, ISchoolTransport>(
    SchoolTransportActionEnums.getSchoolTransportSuccess,
    (schoolTransport: ISchoolTransport) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        schoolTransport,
    })
);

export const getSchoolTransportError = createAction<ISchoolTransportStateContext>(
    SchoolTransportActionEnums.getSchoolTransportError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All SchoolTransports Actions
export const getAllSchoolTransportsPending = createAction<ISchoolTransportStateContext>(
    SchoolTransportActionEnums.getAllSchoolTransportsPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllSchoolTransportsSuccess = createAction<
    ISchoolTransportStateContext,
    IPagedResult<ISchoolTransportList>
>(
    SchoolTransportActionEnums.getAllSchoolTransportsSuccess,
    (result: IPagedResult<ISchoolTransportList>) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        schoolTransports: result.items,
        totalCount: result.totalCount,
    })
);

export const getAllSchoolTransportsError = createAction<ISchoolTransportStateContext>(
    SchoolTransportActionEnums.getAllSchoolTransportsError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create SchoolTransport Actions
export const createSchoolTransportPending = createAction<ISchoolTransportStateContext>(
    SchoolTransportActionEnums.createSchoolTransportPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createSchoolTransportSuccess = createAction<ISchoolTransportStateContext, ISchoolTransport>(
    SchoolTransportActionEnums.createSchoolTransportSuccess,
    (schoolTransport: ISchoolTransport) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        schoolTransport,
    })
);

export const createSchoolTransportError = createAction<ISchoolTransportStateContext>(
    SchoolTransportActionEnums.createSchoolTransportError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update SchoolTransport Actions
export const updateSchoolTransportPending = createAction<ISchoolTransportStateContext>(
    SchoolTransportActionEnums.updateSchoolTransportPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateSchoolTransportSuccess = createAction<ISchoolTransportStateContext, ISchoolTransport>(
    SchoolTransportActionEnums.updateSchoolTransportSuccess,
    (schoolTransport: ISchoolTransport) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        schoolTransport,
    })
);

export const updateSchoolTransportError = createAction<ISchoolTransportStateContext>(
    SchoolTransportActionEnums.updateSchoolTransportError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete SchoolTransport Actions
export const deleteSchoolTransportPending = createAction<ISchoolTransportStateContext>(
    SchoolTransportActionEnums.deleteSchoolTransportPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteSchoolTransportSuccess = createAction<ISchoolTransportStateContext>(
    SchoolTransportActionEnums.deleteSchoolTransportSuccess,
    () => ({
        isPending: false,
        isSuccess: true,
        isError: false,
    })
);

export const deleteSchoolTransportError = createAction<ISchoolTransportStateContext>(
    SchoolTransportActionEnums.deleteSchoolTransportError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Activate SchoolTransport Actions
export const activateSchoolTransportPending = createAction<ISchoolTransportStateContext>(
    SchoolTransportActionEnums.activateSchoolTransportPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const activateSchoolTransportSuccess = createAction<ISchoolTransportStateContext>(
    SchoolTransportActionEnums.activateSchoolTransportSuccess,
    () => ({
        isPending: false,
        isSuccess: true,
        isError: false,
    })
);

export const activateSchoolTransportError = createAction<ISchoolTransportStateContext>(
    SchoolTransportActionEnums.activateSchoolTransportError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Deactivate SchoolTransport Actions
export const deactivateSchoolTransportPending = createAction<ISchoolTransportStateContext>(
    SchoolTransportActionEnums.deactivateSchoolTransportPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deactivateSchoolTransportSuccess = createAction<ISchoolTransportStateContext>(
    SchoolTransportActionEnums.deactivateSchoolTransportSuccess,
    () => ({
        isPending: false,
        isSuccess: true,
        isError: false,
    })
);

export const deactivateSchoolTransportError = createAction<ISchoolTransportStateContext>(
    SchoolTransportActionEnums.deactivateSchoolTransportError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);
