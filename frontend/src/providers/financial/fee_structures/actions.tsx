import { createAction } from "redux-actions";
import { IFeeStructureStateContext } from "./context";
import { IFeeStructure, IFeeStructureList, IPagedResult, IListResult } from "../shared/interfaces";

export enum FeeStructureActionEnums {
    getFeeStructurePending = "GET_FEE_STRUCTURE_PENDING",
    getFeeStructureSuccess = "GET_FEE_STRUCTURE_SUCCESS",
    getFeeStructureError = "GET_FEE_STRUCTURE_ERROR",

    getAllFeeStructuresPending = "GET_ALL_FEE_STRUCTURES_PENDING",
    getAllFeeStructuresSuccess = "GET_ALL_FEE_STRUCTURES_SUCCESS",
    getAllFeeStructuresError = "GET_ALL_FEE_STRUCTURES_ERROR",

    getByGradeAndYearPending = "GET_FEE_STRUCTURES_BY_GRADE_AND_YEAR_PENDING",
    getByGradeAndYearSuccess = "GET_FEE_STRUCTURES_BY_GRADE_AND_YEAR_SUCCESS",
    getByGradeAndYearError = "GET_FEE_STRUCTURES_BY_GRADE_AND_YEAR_ERROR",

    createFeeStructurePending = "CREATE_FEE_STRUCTURE_PENDING",
    createFeeStructureSuccess = "CREATE_FEE_STRUCTURE_SUCCESS",
    createFeeStructureError = "CREATE_FEE_STRUCTURE_ERROR",

    updateFeeStructurePending = "UPDATE_FEE_STRUCTURE_PENDING",
    updateFeeStructureSuccess = "UPDATE_FEE_STRUCTURE_SUCCESS",
    updateFeeStructureError = "UPDATE_FEE_STRUCTURE_ERROR",

    deleteFeeStructurePending = "DELETE_FEE_STRUCTURE_PENDING",
    deleteFeeStructureSuccess = "DELETE_FEE_STRUCTURE_SUCCESS",
    deleteFeeStructureError = "DELETE_FEE_STRUCTURE_ERROR",

    activateFeeStructurePending = "ACTIVATE_FEE_STRUCTURE_PENDING",
    activateFeeStructureSuccess = "ACTIVATE_FEE_STRUCTURE_SUCCESS",
    activateFeeStructureError = "ACTIVATE_FEE_STRUCTURE_ERROR",

    deactivateFeeStructurePending = "DEACTIVATE_FEE_STRUCTURE_PENDING",
    deactivateFeeStructureSuccess = "DEACTIVATE_FEE_STRUCTURE_SUCCESS",
    deactivateFeeStructureError = "DEACTIVATE_FEE_STRUCTURE_ERROR",
}

// Get Single FeeStructure Actions
export const getFeeStructurePending = createAction<IFeeStructureStateContext>(
    FeeStructureActionEnums.getFeeStructurePending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getFeeStructureSuccess = createAction<IFeeStructureStateContext, IFeeStructure>(
    FeeStructureActionEnums.getFeeStructureSuccess,
    (feeStructure: IFeeStructure) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        feeStructure,
    })
);

export const getFeeStructureError = createAction<IFeeStructureStateContext>(
    FeeStructureActionEnums.getFeeStructureError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All FeeStructures Actions
export const getAllFeeStructuresPending = createAction<IFeeStructureStateContext>(
    FeeStructureActionEnums.getAllFeeStructuresPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllFeeStructuresSuccess = createAction<
    IFeeStructureStateContext,
    IPagedResult<IFeeStructureList>
>(
    FeeStructureActionEnums.getAllFeeStructuresSuccess,
    (result: IPagedResult<IFeeStructureList>) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        feeStructures: result.items,
        totalCount: result.totalCount,
    })
);

export const getAllFeeStructuresError = createAction<IFeeStructureStateContext>(
    FeeStructureActionEnums.getAllFeeStructuresError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Grade And Year Actions
export const getByGradeAndYearPending = createAction<IFeeStructureStateContext>(
    FeeStructureActionEnums.getByGradeAndYearPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByGradeAndYearSuccess = createAction<
    IFeeStructureStateContext,
    IListResult<IFeeStructureList>
>(
    FeeStructureActionEnums.getByGradeAndYearSuccess,
    (result: IListResult<IFeeStructureList>) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        feeStructures: result.items,
    })
);

export const getByGradeAndYearError = createAction<IFeeStructureStateContext>(
    FeeStructureActionEnums.getByGradeAndYearError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create FeeStructure Actions
export const createFeeStructurePending = createAction<IFeeStructureStateContext>(
    FeeStructureActionEnums.createFeeStructurePending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createFeeStructureSuccess = createAction<IFeeStructureStateContext, IFeeStructure>(
    FeeStructureActionEnums.createFeeStructureSuccess,
    (feeStructure: IFeeStructure) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        feeStructure,
    })
);

export const createFeeStructureError = createAction<IFeeStructureStateContext>(
    FeeStructureActionEnums.createFeeStructureError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update FeeStructure Actions
export const updateFeeStructurePending = createAction<IFeeStructureStateContext>(
    FeeStructureActionEnums.updateFeeStructurePending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateFeeStructureSuccess = createAction<IFeeStructureStateContext, IFeeStructure>(
    FeeStructureActionEnums.updateFeeStructureSuccess,
    (feeStructure: IFeeStructure) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        feeStructure,
    })
);

export const updateFeeStructureError = createAction<IFeeStructureStateContext>(
    FeeStructureActionEnums.updateFeeStructureError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete FeeStructure Actions
export const deleteFeeStructurePending = createAction<IFeeStructureStateContext>(
    FeeStructureActionEnums.deleteFeeStructurePending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteFeeStructureSuccess = createAction<IFeeStructureStateContext>(
    FeeStructureActionEnums.deleteFeeStructureSuccess,
    () => ({
        isPending: false,
        isSuccess: true,
        isError: false,
    })
);

export const deleteFeeStructureError = createAction<IFeeStructureStateContext>(
    FeeStructureActionEnums.deleteFeeStructureError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Activate FeeStructure Actions
export const activateFeeStructurePending = createAction<IFeeStructureStateContext>(
    FeeStructureActionEnums.activateFeeStructurePending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const activateFeeStructureSuccess = createAction<IFeeStructureStateContext, IFeeStructure>(
    FeeStructureActionEnums.activateFeeStructureSuccess,
    (feeStructure: IFeeStructure) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        feeStructure,
    })
);

export const activateFeeStructureError = createAction<IFeeStructureStateContext>(
    FeeStructureActionEnums.activateFeeStructureError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Deactivate FeeStructure Actions
export const deactivateFeeStructurePending = createAction<IFeeStructureStateContext>(
    FeeStructureActionEnums.deactivateFeeStructurePending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deactivateFeeStructureSuccess = createAction<IFeeStructureStateContext, IFeeStructure>(
    FeeStructureActionEnums.deactivateFeeStructureSuccess,
    (feeStructure: IFeeStructure) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        feeStructure,
    })
);

export const deactivateFeeStructureError = createAction<IFeeStructureStateContext>(
    FeeStructureActionEnums.deactivateFeeStructureError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);
