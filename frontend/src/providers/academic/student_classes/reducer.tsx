import { handleActions } from "redux-actions";
import { INITIAL_STATE, IStudentClassStateContext } from "./context";
import { StudentClassActionEnums } from "./actions";

export const StudentClassReducer = handleActions<
  IStudentClassStateContext,
  IStudentClassStateContext
>(
  {
    [StudentClassActionEnums.getStudentClassPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.getStudentClassSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.getStudentClassError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.getByStudentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.getByStudentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.getByStudentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.getByClassPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.getByClassSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.getByClassError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.getCurrentByStudentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.getCurrentByStudentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.getCurrentByStudentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.enrollPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.enrollSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.enrollError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.updatePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.updateSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.updateError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.endEnrollmentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.endEnrollmentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.endEnrollmentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.setAsCurrentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.setAsCurrentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.setAsCurrentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.deleteStudentClassPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.deleteStudentClassSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentClassActionEnums.deleteStudentClassError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
