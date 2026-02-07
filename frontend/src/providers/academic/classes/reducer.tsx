import { handleActions } from "redux-actions";
import { INITIAL_STATE, IClassStateContext } from "./context";
import { ClassActionEnums } from "./actions";

export const ClassReducer = handleActions<
  IClassStateContext,
  IClassStateContext
>(
  {
    [ClassActionEnums.getClassesPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.getClassesSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.getClassesError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.getClassPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.getClassSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.getClassError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.createClassPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.createClassSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.createClassError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.updateClassPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.updateClassSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.updateClassError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.deleteClassPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.deleteClassSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.deleteClassError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.getActiveClassesPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.getActiveClassesSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.getActiveClassesError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.getClassesByGradePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.getClassesByGradeSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.getClassesByGradeError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.getClassesByAcademicYearPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.getClassesByAcademicYearSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.getClassesByAcademicYearError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.assignClassTeacherPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.assignClassTeacherSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.assignClassTeacherError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.removeClassTeacherPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.removeClassTeacherSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.removeClassTeacherError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.activateClassPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.activateClassSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.activateClassError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.deactivateClassPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.deactivateClassSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ClassActionEnums.deactivateClassError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
