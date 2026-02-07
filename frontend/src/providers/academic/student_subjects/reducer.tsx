import { handleActions } from "redux-actions";
import { INITIAL_STATE, IStudentSubjectStateContext } from "./context";
import { StudentSubjectActionEnums } from "./actions";

export const StudentSubjectReducer = handleActions<
  IStudentSubjectStateContext,
  IStudentSubjectStateContext
>(
  {
    [StudentSubjectActionEnums.getByStudentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentSubjectActionEnums.getByStudentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentSubjectActionEnums.getByStudentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentSubjectActionEnums.getBySubjectPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentSubjectActionEnums.getBySubjectSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentSubjectActionEnums.getBySubjectError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentSubjectActionEnums.getByStudentAndYearPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentSubjectActionEnums.getByStudentAndYearSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentSubjectActionEnums.getByStudentAndYearError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentSubjectActionEnums.enrollPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentSubjectActionEnums.enrollSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentSubjectActionEnums.enrollError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentSubjectActionEnums.unenrollPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentSubjectActionEnums.unenrollSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [StudentSubjectActionEnums.unenrollError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
