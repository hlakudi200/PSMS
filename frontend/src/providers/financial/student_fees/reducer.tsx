import { handleActions } from "redux-actions";
import { INITIAL_STATE, IStudentFeeStateContext } from "./context";
import { StudentFeeActionEnums } from "./actions";

export const StudentFeeReducer = handleActions<
  IStudentFeeStateContext,
  IStudentFeeStateContext
>(
  {
    [StudentFeeActionEnums.getStudentFeePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentFeeActionEnums.getStudentFeeSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentFeeActionEnums.getStudentFeeError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentFeeActionEnums.getAllStudentFeesPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.getAllStudentFeesSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.getAllStudentFeesError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.getByStudentPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.getByStudentSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.getByStudentError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.createStudentFeePending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.createStudentFeeSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.createStudentFeeError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.bulkCreatePending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.bulkCreateSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.bulkCreateError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.updateStudentFeePending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.updateStudentFeeSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.updateStudentFeeError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.deleteStudentFeePending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.deleteStudentFeeSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.deleteStudentFeeError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.applyDiscountPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.applyDiscountSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.applyDiscountError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.waiveFeePending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.waiveFeeSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.waiveFeeError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.cancelFeePending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.cancelFeeSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.cancelFeeError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.checkOverdueFeesPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.checkOverdueFeesSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [StudentFeeActionEnums.checkOverdueFeesError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
  },
  INITIAL_STATE
);
