import { handleActions } from "redux-actions";
import { INITIAL_STATE, type ISubjectsStateContext } from "./context";
import { SubjectsActionEnums } from "./actions";

export const SubjectsReducer = handleActions<ISubjectsStateContext, any>(
  Object.fromEntries(
    Object.values(SubjectsActionEnums).map((action) => [
      action,
      (state: ISubjectsStateContext, a: { payload: Partial<ISubjectsStateContext> }) => ({ ...state, ...a.payload }),
    ])
  ),
  INITIAL_STATE
);
