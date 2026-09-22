import { createAction } from "redux-actions";
import type { IMySubject, ISubjectsStateContext } from "./context";

type SubjectsPatch = Partial<ISubjectsStateContext>;

export enum SubjectsActionEnums {
  getPending = "SUBJECTS_GET_PENDING",
  getSuccess = "SUBJECTS_GET_SUCCESS",
  getError = "SUBJECTS_GET_ERROR",
}

export const getPending = createAction<SubjectsPatch>(SubjectsActionEnums.getPending, () => ({
  isPending: true, isError: false,
}));

export const getSuccess = createAction<SubjectsPatch, IMySubject[]>(
  SubjectsActionEnums.getSuccess,
  (subjects) => ({ isPending: false, isError: false, subjects })
);

export const getError = createAction<SubjectsPatch>(SubjectsActionEnums.getError, () => ({
  isPending: false, isError: true,
}));
