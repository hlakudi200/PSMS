import { handleActions } from "redux-actions";
import { INITIAL_STATE, IFeeStructureStateContext } from "./context";
import { FeeStructureActionEnums } from "./actions";

export const FeeStructureReducer = handleActions<
  IFeeStructureStateContext,
  IFeeStructureStateContext
>(
  {
    [FeeStructureActionEnums.getFeeStructurePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [FeeStructureActionEnums.getFeeStructureSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [FeeStructureActionEnums.getFeeStructureError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [FeeStructureActionEnums.getAllFeeStructuresPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.getAllFeeStructuresSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.getAllFeeStructuresError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.getByGradeAndYearPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.getByGradeAndYearSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.getByGradeAndYearError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.createFeeStructurePending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.createFeeStructureSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.createFeeStructureError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.updateFeeStructurePending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.updateFeeStructureSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.updateFeeStructureError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.deleteFeeStructurePending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.deleteFeeStructureSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.deleteFeeStructureError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.activateFeeStructurePending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.activateFeeStructureSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.activateFeeStructureError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.deactivateFeeStructurePending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.deactivateFeeStructureSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [FeeStructureActionEnums.deactivateFeeStructureError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
  },
  INITIAL_STATE
);
