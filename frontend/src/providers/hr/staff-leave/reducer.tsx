import { handleActions } from "redux-actions";
import { INITIAL_STATE, IStaffLeaveRequestStateContext } from "./context";
import { StaffLeaveActionEnums } from "./actions";

export const StaffLeaveReducer = handleActions<
  IStaffLeaveRequestStateContext,
  IStaffLeaveRequestStateContext
>(
  {
    [StaffLeaveActionEnums.getLeavePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.getLeaveSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.getLeaveError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.getLeavesPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.getLeavesSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.getLeavesError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.createLeavePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.createLeaveSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.createLeaveError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.updateLeavePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.updateLeaveSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.updateLeaveError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.submitPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.submitSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.submitError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.approvePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.approveSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.approveError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.rejectPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.rejectSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.rejectError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.cancelPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.cancelSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StaffLeaveActionEnums.cancelError]: (state, action) => ({
      ...state, ...action.payload,
    }),
  },
  INITIAL_STATE
);
