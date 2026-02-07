import { handleActions } from "redux-actions";
import { INITIAL_STATE, IClassSubjectStateContext } from "./context";
import { ClassSubjectActionEnums } from "./actions";

export const ClassSubjectReducer = handleActions<
  IClassSubjectStateContext,
  IClassSubjectStateContext
>(
  {
    [ClassSubjectActionEnums.getClassSubjectsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.getClassSubjectsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.getClassSubjectsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.getClassSubjectPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.getClassSubjectSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.getClassSubjectError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.getByClassPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.getByClassSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.getByClassError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.getByTeacherPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.getByTeacherSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.getByTeacherError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.createClassSubjectPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.createClassSubjectSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.createClassSubjectError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.updateClassSubjectPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.updateClassSubjectSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.updateClassSubjectError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.deleteClassSubjectPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.deleteClassSubjectSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.deleteClassSubjectError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.assignTeacherPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.assignTeacherSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.assignTeacherError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.removeTeacherPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.removeTeacherSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassSubjectActionEnums.removeTeacherError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
